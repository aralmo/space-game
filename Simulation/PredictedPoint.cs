public class PredictedPoint
{
    public DateTime Time;
    public Vector3D Position;
    public Vector3D Velocity;
    public CelestialBody? MajorInfluence;
    internal bool Accelerating;
}