using static Constants;
public class CelestialBody
{
    #region orbit
    Func<DateTime, Vector3> positionF;
    public float Mass { get; private set; }
    public Vector3 GetPosition(DateTime time) => positionF(time);
    public float GravitationalParameter { get => G * Mass; }
    public Vector3[]? OrbitPoints {get; private set;}
    public OrbitParameters? OrbitParameters {get;private set;}
    public CelestialBody? CentralBody {get; private set;}
    private CelestialBody(Func<DateTime, Vector3> positionF, float mass)
    {
        this.positionF = positionF;
        Mass = mass;
    }

    /// <summary>
    /// Creates a new celestial body on a circular orbit around central body with radius' SemiMajorAxis
    /// </summary>
    /// <param name="centralBody"></param>
    /// <param name="radius"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static CelestialBody Create(CelestialBody centralBody, float radius, float mass, float? eccentricity = null, float? inclination = null, float? argumentOfPeriapsis = null)
    {
        var orbit = Solve.CircularOrbit(radius, centralBody.Mass, default);
        if (eccentricity != null) orbit.Eccentricity = eccentricity.Value;
        if (inclination != null) orbit.Inclination = inclination.Value;
        if (argumentOfPeriapsis != null) orbit.ArgumentOfPeriapsis = argumentOfPeriapsis.Value;
        return Create(centralBody, orbit, mass);
    }
    /// <summary>
    /// Creates a static celestial body, meaning it doesn't orbit any other body.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static CelestialBody Create(Vector3 position, float mass)
        => new CelestialBody(_ => position, mass);
    /// <summary>
    /// Creates a new celestial body that is orbiting another central body.
    /// </summary>
    /// <param name="centralBody"></param>
    /// <param name="parameters"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static CelestialBody Create(CelestialBody centralBody, OrbitParameters parameters, float mass)
        => new CelestialBody(time => parameters.PositionAtTime(time) + centralBody.GetPosition(time), mass)
        {
            OrbitPoints = (parameters.Type == OrbitType.Elliptical)
                ? Solve.OrbitPoints(parameters, 100).ToArray() 
                : null,

            OrbitParameters = parameters,
            CentralBody = centralBody
        };
    #endregion

    #region Visuals
    public float Size { get; private set; } = 1f;
    public Model? Model { get; private set; }
    public CelestialBody WithModelVisuals(Model? model, float size)
    {
        this.Model = model;
        this.Size = size;
        return this;
    }
    #endregion
}
