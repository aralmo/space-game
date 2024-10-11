public class PathPrediction
{
    const float delta = 1f / TARGET_FPS;
    private readonly Simulation sim;
    private readonly DynamicSimulation dsim;
    List<PredictedPoint> points;
    public IEnumerable<PredictedPoint> Points => points;
    public IEnumerable<Maneuver> Maneuvers => maneuvers;
    List<Maneuver> maneuvers = new();
    public PathPrediction(Simulation sim, DynamicSimulation dsim)
    {
        this.sim = sim;
        this.dsim = dsim;
        points = new();
        Predict(sim, dsim.Position, dsim.Velocity, dsim.MajorInfluenceBody, sim.Time);
    }

    private void Predict(Simulation sim, Vector3D position, Vector3D velocity, CelestialBody? majorInfluence, DateTime startTime)
    {
        if (majorInfluence == null)
        {
            majorInfluence = sim.OrbitingBodies.Where(b => b is CelestialBody)
                .Select(b => (influence: Solve.Influence(position, b.GetPosition(startTime), b.Mass), body: b as CelestialBody))
                .MaxBy(b => b.influence).body;
        }
        var predictionTime = Math.Max(40, ExpectedPredictionLengthSeconds(position, velocity, majorInfluence, startTime));
        points.AddRange(YieldPredictions(
            sim: sim,
            position: position,
            velocity: velocity,
            relTime: startTime,
            seconds: (int)predictionTime));
    }

    public void AddManeuver(DateTime time)
    {
        var last = maneuvers.LastOrDefault();
        if (last != null && time <= last.Time) return;
        var m = new Maneuver()
        {
            Time = time
        };
        maneuvers.Add(m);
        UpdatePredictionForManeuver(m);
    }
    public void AddDeltaV(Vector3D deltaV)
    {
        var m = maneuvers.LastOrDefault();
        if (m == null) return;
        m.DeltaV += deltaV;
        UpdatePredictionForManeuver(m);
    }

    private void UpdatePredictionForManeuver(Maneuver? m)
    {
        if (m == null) return;
        points.RemoveAll(p => p.Time > m.Time);
        var last = points.Last();
        Predict(sim, last.Position, last.Velocity, last.MajorInfluence, m.Time);
    }

    IEnumerable<PredictedPoint> YieldPredictions(Simulation sim, Vector3D position, Vector3D velocity, DateTime relTime, int seconds)
    {
        for (int i = 0; i < seconds * TARGET_FPS; i++)
        {
            CelestialBody? majorInfluence = default;
            float topInfluence = float.MinValue;
            relTime = relTime.AddSeconds(delta);
            foreach (CelestialBody body in sim.OrbitingBodies.Where(b => b is CelestialBody))
            {
                var influence = Solve.Influence(position, body.GetPosition(relTime), body.Mass);
                if (influence > MIN_INFLUENCE)
                {
                    if (topInfluence < influence) { majorInfluence = body; topInfluence = influence; }
                    var direction = (body.GetPosition(relTime) - position).Normalize();
                    velocity += direction * influence * delta;
                }
            }

            foreach (var man in maneuvers)
            {
                velocity += man.DVAtTime(relTime, 1) * delta;
            }

            position += velocity * delta;
            yield return new PredictedPoint()
            {
                Position = position,
                Velocity = velocity,
                MajorInfluence = majorInfluence,
                Time = relTime
            };
        }
    }
    static float ExpectedPredictionLengthSeconds(Vector3D position, Vector3D velocity, CelestialBody? body, DateTime time)
        => body != null
            ? FindOrbitT(position - body.GetPosition(time), velocity - body.GetVelocity(time), body, time) - 2f
            : 120;
    private static float FindOrbitT(Vector3D position, Vector3D velocity, CelestialBody majorInfluenceBody, DateTime time)
        => (float)Solve.KeplarOrbit(position - majorInfluenceBody.GetPosition(time), velocity, majorInfluenceBody.Mass, time).T;
    public void Reset()
    {
        points.Clear();
        Predict(sim, dsim.Position, dsim.Velocity, dsim.MajorInfluenceBody, sim.Time);
    }
    public void RemoveManeuver()
    {
        maneuvers.RemoveAt(maneuvers.Count - 1);
        var man = maneuvers.LastOrDefault();
        if (man == null)
        {
            points.Clear();
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
    public Vector3D DVAtTime(DateTime time, float acceleration)
    {
        var remaining = DeltaV.Magnitude() - ((time - Time).TotalSeconds * acceleration);
        if (remaining < 0) return Vector3D.Zero;
        var force = Math.Min(remaining, acceleration);
        return DeltaV.Normalize() * force;
    }
}