using static Constants;
public class CelestialBody
{
    Func<DateTime, Vector3> positionF;
    public float Mass { get; private set; }
    public Vector3 GetPosition(DateTime time) => positionF(time);
    public float GravitationalParameter { get => G * Mass; }
    private CelestialBody(Func<DateTime, Vector3> positionF, float mass)
    {
        this.positionF = positionF;
        Mass = mass;
    }

    /// <summary>
    /// Creates a static celestial body, meaning it doesn't orbit any other body.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public CelestialBody Create(Vector3 position, float mass)
        => new CelestialBody(_ => position, mass);

    /// <summary>
    /// Creates a new celestial body that is orbiting another central body.
    /// </summary>
    /// <param name="centralBody"></param>
    /// <param name="parameters"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public CelestialBody Create(CelestialBody centralBody, OrbitParameters parameters, float mass)
        => new CelestialBody(time => parameters.PositionAtTime(time) + centralBody.GetPosition(time), mass);

}
