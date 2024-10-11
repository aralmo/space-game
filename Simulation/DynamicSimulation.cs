using System.Security.Cryptography.X509Certificates;

public class DynamicSimulation
{
    internal readonly Simulation simulation;
    public Vector3D Position { get; set; }
    public Vector3D Velocity { get; set; }
    public CelestialBody? MajorInfluenceBody { get; set; }
    public DynamicSimulation(Simulation simulation, Vector3D position, Vector3D velocity)
    {
        this.simulation = simulation;
        Position = position;
        Velocity = velocity;
    }
    public Vector3 UpVector()
    {
        var forward = new Vector3(0, 0, 1); // Assuming forward direction is along the Z-axis
        var velocityVector = new Vector3((float)Velocity.X, (float)Velocity.Y, (float)Velocity.Z);
        return - Vector3.Cross(forward, velocityVector).Normalize();
    }
    public void Draw3D(Model model)
    {
        var v = Velocity - (MajorInfluenceBody != null ? MajorInfluenceBody.GetVelocity(simulation.Time) : Vector3D.Zero);
        var forward = new Vector3(0, 0, 1); // Assuming forward direction is along the Z-axis
        var velocityVector = new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        var rotationAxis = Vector3.Cross(forward, velocityVector).Normalize();
        var degrees = MathF.Acos(Vector3.Dot(forward.Normalize(), velocityVector.Normalize())) * (180 / MathF.PI);

        DrawModelEx(model, Position, rotationAxis, degrees, new Vector3(1, 1, 1), Color.White);
    }
}
