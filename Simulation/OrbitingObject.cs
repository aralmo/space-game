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

    public bool InHierarchy(OrbitingObject o) => o == this || (CentralBody != null && CentralBody.InHierarchy(o));
    public double GravitationalParameter { get => G * Mass; }
    public Vector3D[]? OrbitPoints { get; protected set; }
    public OrbitParameters? OrbitParameters { get; protected set; }
    public OrbitingObject? CentralBody { get; protected set; }
    protected OrbitingObject(Func<DateTime, Vector3D> positionF, double mass)
    {
        this.positionF = positionF;
        Mass = mass;
    }

    public static OrbitingObject Create(OrbitingObject centralBody, double radius, double mass,DateTime time, double? eccentricity = null, double? inclination = null, double? argumentOfPeriapsis = null)
    {
        var orbit = Solve.CircularOrbit(radius, centralBody.Mass, time);
        if (eccentricity != null) orbit.Eccentricity = eccentricity.Value;
        if (inclination != null) orbit.Inclination = inclination.Value;
        if (argumentOfPeriapsis != null) orbit.ArgumentOfPeriapsis = argumentOfPeriapsis.Value;
        return Create(centralBody, orbit, mass);
    }
    public static OrbitingObject Create(OrbitingObject centralBody, OrbitParameters parameters, double mass)
    => new OrbitingObject(time => parameters.PositionAtTime(time), mass)
    {
        OrbitPoints = (parameters.Type == OrbitType.Elliptical)
            ? Solve.OrbitPoints(parameters, (int)parameters.SemiMajorAxis * 2).ToArray()
            : null,

        OrbitParameters = parameters,
        CentralBody = centralBody
    };
    #endregion
}