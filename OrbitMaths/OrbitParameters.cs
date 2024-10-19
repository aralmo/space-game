using System.Security.Cryptography.X509Certificates;

public struct OrbitParameters
{
    public OrbitType Type;
    public double T;
    public double SemiMajorAxis;        // Semi-major axis, for elliptical orbits only (a)
    public double Eccentricity;         // Eccentricity of the orbit (e)
    public double Inclination;          // Inclination of the orbit (i)
    public double LongitudeOfAscendingNode; // Longitude of ascending node (Ω)
    public double ArgumentOfPeriapsis;  // Argument of periapsis (ω)
    public double TrueAnomaly;          // True anomaly (ν)
    public double MeanAnomaly;          // Mean anomaly (M)
    public double TimeOfPeriapsisPassage;  // Time of periapsis passage (T0)
    public double GravitationalParameter; // Gravitational parameter (μ)
    public DateTime EpochTime;          // Epoch time of the orbital parameters
    public Vector3D AsymptoteDirection;  // For hyperbolic orbits

    public IEnumerable<(string par, object value)> Parameters()
    {
        yield return ("T", T);
        yield return ("SMa", SemiMajorAxis);
        yield return ("Ecc", Eccentricity);
        yield return ("Inc", Inclination);
        yield return ("O", LongitudeOfAscendingNode);
        yield return ("W", ArgumentOfPeriapsis);
        yield return ("v", TrueAnomaly);
        yield return ("M", MeanAnomaly);
        yield return ("TPe", TimeOfPeriapsisPassage);
        yield return ("Gp", GravitationalParameter);
        if (Type == OrbitType.Hyperbolic || Type == OrbitType.Parabolic){
            yield return ("AsD", AsymptoteDirection);
        }
    }

    
}

public struct OrbitLocationParameters
{
    public OrbitLocationParameters(OrbitParameters p){
        this.p = p;
    }
    public double PeT => p.T - (double)(DateTime.UtcNow - p.EpochTime).TotalSeconds - p.TimeOfPeriapsisPassage;
    public double ApT;
    private readonly OrbitParameters p;
} 

public enum OrbitType
{
    Elliptical,
    Parabolic,
    Hyperbolic,
}
