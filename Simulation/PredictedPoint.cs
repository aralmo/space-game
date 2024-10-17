public record PredictedPoint
{
    public DateTime Time;
    public Vector3D Position;
    public Vector3D Velocity;
    public bool IsCollision;
    public bool IsJoin;
    public CelestialBody? MajorInfluence;
    internal bool Accelerating;
    internal bool IsCapturing;
}