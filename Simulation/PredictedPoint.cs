public record PredictedPoint
{
    public DateTime Time;
    public Vector3D Position;
    public Vector3D Velocity;
    public bool IsCollision;
    public bool IsJoin;
    public StationaryOrbitObject? JoinObject;
    public CelestialBody? MajorInfluence;
    internal float TimeAccelerating;
}