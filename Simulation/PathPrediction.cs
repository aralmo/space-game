using System.IO.Compression;

public class PathPrediction
{
    const float delta = 1f / SIM_FPS;
    private readonly Simulation sim;
    private readonly DynamicSimulation dsim;
    List<PredictedPoint> points;
    public IEnumerable<(OrbitingObject obj, DateTime time, float distance)> ClosestEncounters => closestFlybys.Select(cfb => (cfb.Key, cfb.Value.time, cfb.Value.distance));
    public IEnumerable<PredictedPoint> Points => points;
    public IEnumerable<Maneuver> Maneuvers => maneuvers;
    List<Maneuver> maneuvers = new();
    Dictionary<OrbitingObject, (float distance, DateTime time)> closestFlybys = new();

    public PathPrediction(Simulation sim, DynamicSimulation dsim)
    {
        this.sim = sim;
        this.dsim = dsim;
        points = new();
        Predict(sim, dsim.Position, dsim.Velocity, dsim.MajorInfluenceBody, sim.Time);
    }

    private void Predict(Simulation sim, Vector3D position, Vector3D velocity, CelestialBody? majorInfluence, DateTime startTime)
    {
        //calculate prediction length
        var predictionStart = Game.Simulation.Time;
        var validPoints = points.Where(p => p.Time >= Game.Simulation.Time);
        if (validPoints.LastOrDefault()?.IsCollision ?? false) return;
        var predictionEnd = validPoints.LastOrDefault()?.Time ?? Game.Simulation.Time;
        var transferPoints = Transfers(validPoints).ToArray();
        var lastTransferPoint = transferPoints.LastOrDefault();
        var lastManeuver = maneuvers.LastOrDefault(m
            => m.Time >= Game.Simulation.Time && (lastTransferPoint == null || m.Time > lastTransferPoint.Time));

        var significantPoint = lastManeuver?.Point ?? lastTransferPoint ?? validPoints.FirstOrDefault(p => p.TimeAccelerating > 0);
        var predictionEndPoint = significantPoint;
        var expectedPredictionEnd = predictionEndPoint?.Time ?? Game.Simulation.Time;
        if (lastManeuver != null)
        {
            expectedPredictionEnd = expectedPredictionEnd.AddSeconds(lastManeuver.BurnTime(SHIP_ACCELERATION));
        }
        if (transferPoints.Length < 2)
        {
            var influence = significantPoint?.MajorInfluence;
            var ship = Game.PlayerShip.DynamicSimulation;
            if (influence == null)
            {
                influence = Game.Simulation.OrbitingBodies
                    .Where(o => o is CelestialBody)
                    .MaxBy(o => Solve.Influence(ship.Position, o.GetPosition(Game.Simulation.Time), o.Mass))
                    as CelestialBody;
            }
            if (influence != null)
            {
                var sp = significantPoint?.Position ?? ship.Position;
                var sv = significantPoint?.Velocity ?? ship.Velocity;
                if (lastManeuver != null)
                {
                    sv += lastManeuver.DeltaV;
                }
                var t = FindOrbitT(sp, sv, influence, significantPoint?.Time ?? Game.Simulation.Time);
                if (t != null)
                {
                    expectedPredictionEnd = expectedPredictionEnd.AddSeconds(t.Value);
                }
                else
                {
                    var last = maneuvers.LastOrDefault();
                    if (last != null)
                    {
                        expectedPredictionEnd = last.Time.AddSeconds(last.BurnTime(SHIP_ACCELERATION));
                        var ctime = expectedPredictionEnd - (points.FirstOrDefault()?.Time ?? Game.Simulation.Time);
                        expectedPredictionEnd = expectedPredictionEnd.Add(ctime);
                    }
                    else
                    {
                        expectedPredictionEnd = expectedPredictionEnd.AddSeconds(100);
                    }
                }
            }
        }
        else
        {
            expectedPredictionEnd = transferPoints.Last().Time.AddSeconds(60);
        }
        if (expectedPredictionEnd == Game.Simulation.Time)
        {
            expectedPredictionEnd = expectedPredictionEnd.AddSeconds(120);
        }
        var remaining = expectedPredictionEnd - predictionEnd;
        if (remaining.TotalSeconds > 0)
        {
            points.AddRange(YieldPredictions(
                sim: sim,
                closestFlybys: closestFlybys,
                position: position,
                velocity: velocity,
                relTime: startTime,
                seconds: (int)Math.Min(60, remaining.TotalSeconds)));
        }
    }

    Task predictionTask;
    public void StartAsync()
    {
        predictionTask = Task.Run(()=>{

        });
    }

    static IEnumerable<PredictedPoint> Transfers(IEnumerable<PredictedPoint> points)
    {
        if (points == null || !points.Any()) yield break;
        CelestialBody? body = points.First().MajorInfluence;
        foreach (var point in points)
        {
            if (point.MajorInfluence != body)
            {
                yield return point;
                body = point.MajorInfluence;
            }
        }
    }
    public void AddManeuver(Maneuver maneuver)
    {
        var last = maneuvers.LastOrDefault();
        if (last != null && maneuver.Time <= last.Time) return;
        if (maneuver.Point == null)
        {
            maneuver.Point = points.FirstOrDefault(p => p.Time == maneuver.Time);
        }
        maneuvers.Add(maneuver);
        UpdatePredictionForManeuver(maneuver);
    }
    public void AddManeuver(DateTime time)
    {
        var m = new Maneuver()
        {
            Time = time
        };
        AddManeuver(m);
    }
    public void AddDeltaV(Vector3D deltaV)
    {
        var m = maneuvers.LastOrDefault();
        if (m == null) return;
        m.DeltaV += deltaV;
        UpdatePredictionForManeuver(m);
    }
    public void Update()
    {
        var point = points.LastOrDefault();
        Predict(sim, point.Position, point.Velocity, point.MajorInfluence, point.Time);
    }
    private void UpdatePredictionForManeuver(Maneuver? m)
    {
        if (m == null) return;
        points.RemoveAll(p => p.Time > m.Time);
        RemoveAfterCloseBy(m);
        var last = points.LastOrDefault();
        if (last == null) return;
        Predict(sim, last.Position, last.Velocity, last.MajorInfluence, m.Time);
    }

    private void RemoveAfterCloseBy(Maneuver? m)
    {
        var ctr = closestFlybys.Where(p => p.Value.time > m.Time).ToArray();
        foreach (var c in ctr)
        {
            closestFlybys.Remove(c.Key);
        }
    }

    IEnumerable<PredictedPoint> YieldPredictions(Simulation sim, Vector3D position, Vector3D velocity, DateTime relTime, int seconds, Dictionary<OrbitingObject, (float distance, DateTime time)> closestFlybys)
    {
        float timeAccelerating = points.LastOrDefault()?.TimeAccelerating ?? 0;
        for (int i = 0; i < seconds * SIM_FPS; i++)
        {
            CelestialBody? majorInfluence = default;
            float topInfluence = float.MinValue;
            relTime = relTime.AddSeconds(delta);
            bool colliding = false;
            foreach (var man in maneuvers)
            {
                var manForce = man.DVAtTime(relTime, Constants.SHIP_ACCELERATION) * delta;
                if (manForce.Magnitude() > 0)
                {
                    timeAccelerating += delta;
                }
                else
                {
                    timeAccelerating = 0;
                }
                velocity += manForce;
            }
            foreach (var obody in sim.OrbitingBodies)
            {
                var bodypos = obody.GetPosition(relTime);
                var distance = (float)Vector3D.Distance(position, bodypos);
                if (closestFlybys.TryGetValue(obody, out (float distance, DateTime time) flyby))
                {
                    if (flyby.distance > distance || flyby.time < Game.Simulation.Time)
                    {
                        closestFlybys[obody] = (distance, relTime);
                    }
                }
                else
                {
                    closestFlybys.Add(obody, (distance, relTime));
                }
                if (obody is CelestialBody body)
                {
                    var influence = Solve.Influence(position, bodypos, body.Mass);
                    if (influence > MIN_INFLUENCE)
                    {
                        if (topInfluence < influence) { majorInfluence = body; topInfluence = influence; }
                        var direction = (body.GetPosition(relTime) - position).Normalize();
                        velocity += direction * influence * delta;
                        colliding = IsColliding(position, body, relTime);
                    }
                }
            }

            position += velocity * delta;

            yield return new PredictedPoint()
            {
                Position = position,
                Velocity = velocity,
                MajorInfluence = majorInfluence,
                Time = relTime,
                TimeAccelerating = timeAccelerating,
                IsCollision = colliding,
            };
            if (colliding)
            {
                yield break;
            }
        }
    }
    static bool IsColliding(Vector3D point, CelestialBody body, DateTime t)
        => Vector3D.Distance(point, body.GetPosition(t)) < body.Size * 1.1f;
    private static float? FindOrbitT(Vector3D position, Vector3D velocity, CelestialBody majorInfluenceBody, DateTime time)
    {
        var body = majorInfluenceBody;
        var orbit = Solve.KeplarOrbit(position - body.GetPosition(time), velocity - body.GetVelocity(time), body.Mass, time);
        if (orbit.Type == OrbitType.Hyperbolic) return null;
        return (float)orbit.T;
    }
    public void Reset()
    {
        points.Clear();
        closestFlybys.Clear();
        Predict(sim, dsim.Position, dsim.Velocity, dsim.MajorInfluenceBody, sim.Time);
    }
    public void RemoveManeuver()
    {
        maneuvers.RemoveAt(maneuvers.Count - 1);

        var man = maneuvers.LastOrDefault();
        if (man == null)
        {
            Reset();
            Predict(sim, dsim.Position, dsim.Velocity, dsim.MajorInfluenceBody, sim.Time);
        }
        else
        {
            UpdatePredictionForManeuver(man);
        }
    }
}

public class Maneuver
{
    public Vector3D DeltaV;
    public DateTime Time;
    internal PredictedPoint? Point;
    public OrbitingObject? JoinTarget { get; internal set; }

    public Vector3D DVAtTime(DateTime time, float acceleration)
    {
        var remaining = DeltaV.Magnitude() - ((time - Time).TotalSeconds * acceleration);
        if (remaining < 0) return Vector3D.Zero;
        var force = Math.Min(remaining, acceleration);
        return DeltaV.Normalize() * force;
    }
    public float BurnTime(float acceleration)
    {
        return (float)DeltaV.Magnitude() / acceleration;
    }
}