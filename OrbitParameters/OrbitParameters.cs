public struct OrbitParameters
{
    public OrbitType Type;
    public float T;
    public float SemiMajorAxis;        // Semi-major axis, for elliptical orbits only (a)
    public float Eccentricity;         // Eccentricity of the orbit (e)
    public float Inclination;          // Inclination of the orbit (i)
    public float LongitudeOfAscendingNode; // Longitude of ascending node (Ω)
    public float ArgumentOfPeriapsis;  // Argument of periapsis (ω)
    public float TrueAnomaly;          // True anomaly (ν)
    public float MeanAnomaly;          // Mean anomaly (M)
    public float TimeOfPeriapsisPassage;  // Time of periapsis passage (T0)
    public float GravitationalParameter; // Gravitational parameter (μ)
    public DateTime EpochTime;          // Epoch time of the orbital parameters
    public Vector3 AsymptoteDirection;  // For hyperbolic orbits
}

public enum OrbitType
{
    Elliptical,
    Parabolic,
    Hyperbolic,
}
