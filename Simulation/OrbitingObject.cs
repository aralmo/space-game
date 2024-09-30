public class OrbitingObject
{
    #region orbit
    Func<DateTime, Vector3D> positionF;
    public double Mass { get; protected set; }
    public Vector3D GetPosition(DateTime time) 
        => positionF(time) + (CentralBody != null 
            ? CentralBody.GetPosition(time) 
            : Vector3D.Zero);
    public Vector3D GetVelocity(DateTime time) 
        => OrbitParameters != null 
            ? OrbitParameters.Value.VelocityAtTime(time) + CentralBody!.GetVelocity(time) 
            : Vector3D.Zero;
    public double GravitationalParameter { get => G * Mass; }
    public Vector3D[]? OrbitPoints { get; protected set; }
    public OrbitParameters? OrbitParameters { get; protected set; }
    public OrbitingObject? CentralBody { get; protected set; }
    protected OrbitingObject(Func<DateTime, Vector3D> positionF, double mass)
    {
        this.positionF = positionF;
        Mass = mass;
    }
    #endregion
}
