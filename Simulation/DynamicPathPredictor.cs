public class DynamicPathPredictor
{
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
    bool needsUpdate = false;
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

                    var predicted = PredictPath((int)currentOrbitT + ORBIT_SLOW_PREDICT_TIME_SECONDS, TARGET_FPS);
                    Prediction = predicted;
                }

                for (int i = 0; i < ORBIT_SLOW_PREDICT_TIME_SECONDS * 100; i++)
                {
                    Thread.Sleep(10);
                    if (needsUpdate) break;
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
        var tempPosition = dobject.Position;
        var tempVelocity = dobject.Velocity;
        var tempSimulationTime = dobject.simulation.SimulationTime;
        var closestPositions = new Dictionary<OrbitingObject, (double distance, Vector3D pos, Vector3D ship)>();
        bool crashFinish = false;
        int iters = 0;
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
                    tempVelocity += force * (step / fps);
                }
            }

            // Update position based on the new velocity
            tempPosition += tempVelocity * (step / fps);
            tempSimulationTime = tempSimulationTime.AddSeconds(step / fps);

            // Add the new position to the predicted path
            if (dobject.MajorInfluenceBody != null)
            {
                var referencePosition = dobject.MajorInfluenceBody.GetPosition(tempSimulationTime);
                var vector = tempPosition - referencePosition;
                predictedPath.Add(vector);
            }
            else
            {
                predictedPath.Add(tempPosition);
            }
            if (crashFinish) break;
        }

        return new PredictedPath()
        {
            Points = predictedPath.ToArray(),
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
    public void DrawPredictedPath2D(Camera3D camera)
    {
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
                Draw2DLineOfPoints(camera, Prediction.Value.Points.Select(p => p + mibpos).Decimate(400).ToArray(), Color.Beige, false);
            }
        }
    }
}