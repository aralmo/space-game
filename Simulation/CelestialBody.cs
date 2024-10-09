public class CelestialBody : OrbitingObject
{
    #region Factory Methods
    /// <summary>
    /// Creates a new celestial body on a circular orbit around central body with radius' SemiMajorAxis
    /// </summary>
    /// <param name="centralBody"></param>
    /// <param name="radius"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static CelestialBody CreateCelestial(OrbitingObject centralBody, double radius, double mass,DateTime time, double? eccentricity = null, double? inclination = null, double? argumentOfPeriapsis = null)
    {
        var orbit = Solve.CircularOrbit(radius, centralBody.Mass, time);
        if (eccentricity != null) orbit.Eccentricity = eccentricity.Value;
        if (inclination != null) orbit.Inclination = inclination.Value;
        if (argumentOfPeriapsis != null) orbit.ArgumentOfPeriapsis = argumentOfPeriapsis.Value;
        return CreateCelestial(centralBody, orbit, mass);
    }
    /// <summary>
    /// Creates a static celestial body, meaning it doesn't orbit any other body.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static CelestialBody CreateCelestial(Vector3D position, double mass)
        => new CelestialBody(_ => position, mass);
    /// <summary>
    /// Creates a new celestial body that is orbiting another central body.
    /// </summary>
    /// <param name="centralBody"></param>
    /// <param name="parameters"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static CelestialBody CreateCelestial(OrbitingObject centralBody, OrbitParameters parameters, double mass)
        => new CelestialBody(time => parameters.PositionAtTime(time), mass)
        {
            OrbitPoints = (parameters.Type == OrbitType.Elliptical)
                ? Solve.OrbitPoints(parameters, (int)parameters.SemiMajorAxis * 2).ToArray()
                : null,

            OrbitParameters = parameters,
            CentralBody = centralBody
        };
    #endregion

    #region Visuals
    public string? Name { get; protected set; }
    public float Size { get; protected set; } = 1f;
    public Model? Model { get; protected set; }
    public Color FarColor { get; protected set; }
    public CelestialBody WithModelVisuals(Model? model, float size, Color? farColor = null)
    {
        this.Model = model;
        this.Size = size;
        this.FarColor = farColor ?? Color.LightGray;
        return this;
    }

    public CelestialBody WithInfo(string name)
    {
        this.Name = name;
        return this;
    }
    #endregion

    private CelestialBody(Func<DateTime, Vector3D> positionF, double mass) : base(positionF, mass) { }
}
