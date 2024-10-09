public class DynamicPathPredictor
{
    object maneuversLock = new object();
    Maneuver[] _maneuvers = Array.Empty<Maneuver>();
    public Maneuver[] Maneuvers
    {
        get
        {
            lock (maneuversLock)
            {
                return _maneuvers;
            }
        }
        set
        {
            lock (maneuversLock)
            {
                _maneuvers = value;
            }
        }
    }
    private readonly DynamicSimulation dobject;
    object plock = new object();
    PredictedPath? _prediction;
    internal PredictedPath? Prediction
    {
        get
        {
            lock (plock)
            {
                return _prediction;
            }
        }
        set
        {
            lock (plock)
            {
                _prediction = value;
            }
        }
    }
    double currentOrbitT = 120;
    bool needsUpdate = true;
    bool cancelPrediction = false;
    Thread? predictionThread;
    public void Start()
    {
        cancelPrediction = false;
        predictionThread = new Thread(() =>
        {
            while (true)
            {
                if (cancelPrediction) return;
                Thread.Sleep(10);
                if (!needsUpdate) continue;
                if (dobject.MajorInfluenceBody != null)
                {
                    var orbit = Solve.KeplarOrbit(
                        dobject.Position - dobject.MajorInfluenceBody.GetPosition(dobject.simulation.SimulationTime),
                        dobject.Velocity - dobject.MajorInfluenceBody.GetVelocity(dobject.simulation.SimulationTime),
                        dobject.MajorInfluenceBody.Mass
                    );
                    if (orbit.Type == OrbitType.Elliptical)
                    {
                        currentOrbitT = orbit.T;
                    }
                    else
                    {
                        currentOrbitT = 120;
                    }
                    var predicted = PredictPath((int)currentOrbitT + ORBIT_SLOW_PREDICT_TIME_SECONDS, TARGET_FPS);
                    if (predicted.HasValue && predicted.Value.ClosestToBodyPositions.Any())
                    {
                        currentOrbitT+=60;
                    }
                    Prediction = predicted;
                }

                needsUpdate = false;
            }
        });
        predictionThread.IsBackground = true;
        predictionThread.Priority = ThreadPriority.Lowest;
        predictionThread.Start();
    }
    public PredictedPath? PredictPath(int seconds = 120, int fps = 60, int maxIterations = 60000)
    {
        if (dobject.MajorInfluenceBody == null) return Prediction;

        //if (seconds > 2000) seconds = 2000;
        float step = 1f;
        if (seconds * fps > maxIterations)
        {
            step = Math.Min(4, (float)(seconds * fps) / maxIterations);
        }

        var predictedPath = new List<Vector3D>();
        var predictedVelocities = new List<Vector3D>();
        var predictedTimes = new List<DateTime>();
        var tempPosition = dobject.Position;
        var tempVelocity = dobject.Velocity;
        var tempSimulationTime = dobject.simulation.SimulationTime;
        var closestPositions = new Dictionary<OrbitingObject, (double distance, Vector3D pos, Vector3D ship)>();
        bool crashFinish = false;
        int iters = 0;
        var mansDv = Vector3D.Zero;
        for (float t = 0f; t < seconds * fps; t += step)
        {
            if (iters++ > maxIterations) break;
            var porPos = dobject.MajorInfluenceBody!.GetPosition(tempSimulationTime);
            var porDis = Vector3D.Distance(porPos, tempPosition);
            var referenceInfluence = G * dobject.MajorInfluenceBody?.Mass / (porDis * porDis);

            foreach (var obj in dobject.simulation.OrbitingBodies)
            {
                if (obj is CelestialBody body)
                {
                    var bodyPosition = body.GetPosition(tempSimulationTime);
                    var distance = Vector3D.Distance(tempPosition, bodyPosition);
                    var influence = G * body.Mass / (distance * distance);
                    if (influence < MIN_INFLUENCE) continue;

                    if (dobject.MajorInfluenceBody == null || dobject.MajorInfluenceBody != obj)
                    {
                        if (!closestPositions.ContainsKey(obj) || closestPositions[obj].distance > distance)
                        {
                            closestPositions[obj] = (
                                distance,
                                bodyPosition - porPos,
                                tempPosition - porPos);
                        }
                    }
                    // Calculate the gravitational force
                    var direction = (bodyPosition - tempPosition).Normalize();
                    var force = direction * influence;
                    //brake flow conditions
                    if (distance < body.Size)
                    {
                        crashFinish = true;
                        break;
                    }
                    // Update the velocity based on the force
                    mansDv = Maneuvers.Where(m => m.Time <= tempSimulationTime).Select(m => m.DeltaV).Sum();
                    tempVelocity += force * (step / fps);
                }
            }

            // Update position based on the new velocity
            tempPosition += (tempVelocity + mansDv) * (step / fps);
            tempSimulationTime = tempSimulationTime.AddSeconds(step / fps);

            // Add the new position to the predicted path
            if (dobject.MajorInfluenceBody != null)
            {
                var referencePosition = dobject.MajorInfluenceBody.GetPosition(tempSimulationTime);
                var vector = tempPosition - referencePosition;
                predictedPath.Add(vector);
                predictedVelocities.Add(tempVelocity + mansDv);
                predictedTimes.Add(tempSimulationTime);
            }
            else
            {
                predictedPath.Add(tempPosition);
                predictedVelocities.Add(tempVelocity + mansDv);
                predictedTimes.Add(tempSimulationTime);
            }
            if (crashFinish) break;
        }
        return new PredictedPath()
        {
            Positions = predictedPath.ToArray(),
            Velocities = predictedVelocities.ToArray(),
            Times = predictedTimes.ToArray(),
            ClosestToBodyPositions = closestPositions.Select(k => (k.Key, k.Value.pos, k.Value.ship)).ToArray()
        };
    }
    public void Stop()
    {
        cancelPrediction = true;
    }
    public DynamicPathPredictor(DynamicSimulation simulation)
    {
        this.dobject = simulation;
    }
    public void DrawPredictedPath2D(Camera3D camera, out Vector3D? closestPoint)
    {
        closestPoint = default;
        if (Prediction != null)
        {
            var mibpos = dobject.MajorInfluenceBody?.GetPosition(dobject.simulation.SimulationTime) ?? Vector3D.Zero;
            foreach (var closest in Prediction.Value.ClosestToBodyPositions)
            {
                var distance = Vector3D.Distance(camera.Position, closest.BodyPosition);
                double sizeFactor = 1000 / distance;
                float size = 1f;
                if (closest.Body is CelestialBody celestial)
                {
                    size = celestial.Size;
                }
                float drawSize = (float)Math.Max(1f, size * sizeFactor);
                var p1 = closest.BodyPosition + mibpos;
                var p2 = closest.ShipPosition + mibpos;
                if (Vector3D.Distance(p1, p2) < MIN_CAPTURE_DISTANCE)
                {
                    var pos = GetWorldToScreen(p1, camera);
                    var shipP = GetWorldToScreen(p2, camera);
                    DrawCircle((int)float.Round(pos.X), (int)float.Round(pos.Y), Math.Max(4, drawSize), Color.Orange);
                    DrawCircle((int)float.Round(shipP.X), (int)float.Round(shipP.Y), Math.Max(4, drawSize), Color.Blue);
                }
            }
            Draw2DLineOfPoints(camera, Prediction.Value.Positions.Select(p => p + mibpos).Decimate(400).ToArray(), out Vector3D? closestM, Color.Beige, false);
            if (closestM != null)
            {
                closestPoint = closestM.Value + dobject.Position;
            }
        }
    }

    internal void Invalidate()
    {
        needsUpdate = true;
    }
}