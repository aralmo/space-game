using System.Security.Cryptography.X509Certificates;

public class DynamicSimulation
{
    public DynamicPathPredictor PathPredictor {get;private set;}
    internal readonly Simulation simulation;
    public Vector3D Position { get; set; }
    public Vector3D Velocity { get; set; }
    public CelestialBody? MajorInfluenceBody { get; private set; }
    public DynamicSimulation(Simulation simulation, Vector3D position, Vector3D velocity)
    {
        this.simulation = simulation;
        Position = position;
        Velocity = velocity;
        PathPredictor = new DynamicPathPredictor(this);
    }
    public void ApplyGravity(DateTime? time, float deltaTime)
    {
        double maxInfluence = double.MinValue;

        foreach (var obj in simulation.OrbitingBodies)
        {
            if (obj is CelestialBody body)
            {
                var bodyPosition = body.GetPosition(time ?? simulation.SimulationTime);
                var distance = Vector3.Distance(Position, bodyPosition);
                var influence = G * body.Mass / (distance * distance);
                if (influence < MIN_INFLUENCE) continue;
                if (maxInfluence < influence)
                {
                    maxInfluence = influence;
                    MajorInfluenceBody = body;
                }
                (_, Velocity) = Solve.ApplyGravity(
                    position: Position,
                    velocity: Velocity,
                    planetPosition: body.GetPosition(time ?? simulation.SimulationTime),
                    planetMass: body.Mass,
                    stepTimeSeconds: deltaTime);
            }
        }
    }
    public void UpdatePosition(float deltaTime)
    {
        Position += Velocity * deltaTime;
    }



    public void Draw3D(Model model)
    {
        var v = Velocity - (MajorInfluenceBody != null? MajorInfluenceBody.GetVelocity(simulation.SimulationTime) : Vector3D.Zero);
        var forward = new Vector3(0, 0, 1); // Assuming forward direction is along the Z-axis
        var velocityVector = new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        var rotationAxis = Vector3.Cross(forward, velocityVector).Normalize();
        var degrees = MathF.Acos(Vector3.Dot(forward.Normalize(), velocityVector.Normalize())) * (180 / MathF.PI);

        DrawModelEx(model, Position, rotationAxis, degrees, new Vector3(1,1,1), Color.White);
    }
}
