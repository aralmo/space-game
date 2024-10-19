using System.Security.Cryptography.X509Certificates;
using static Constants;

public static class Solve
{
    public static OrbitParameters CircularOrbit(double distance, double bodymass, DateTime date)
    {
        var startPos = new Vector3D(distance, 0f, 0f);
        var vel = new Vector3D(0f, 0f, OrbitVelocity(distance, bodymass));
        return KeplarOrbit(startPos, vel, bodymass, date);
    }
    public static OrbitParameters EllipticalOrbit(double semiMajorAxis, double eccentricity, double centralBodyMass, double argumentOfPeriapsis = 0f, double inclination = 0f)
    {
        double mu = G * centralBodyMass;
        double longitudeOfAscendingNodeRad = Math.Acos(Math.Cos(inclination));
        double argumentOfPeriapsisRad = argumentOfPeriapsis == 0f ? Math.Acos(Math.Cos(longitudeOfAscendingNodeRad) * Math.Cos(inclination)) : argumentOfPeriapsis;
        double trueAnomalyRad = Math.Acos(Math.Cos(argumentOfPeriapsisRad) * Math.Cos(longitudeOfAscendingNodeRad) * Math.Cos(inclination));
        double meanAnomaly = trueAnomalyRad - eccentricity * Math.Sin(trueAnomalyRad);
        double timeOfPeriapsisPassage = meanAnomaly / Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3));
        Vector3D asymptoteDirection = eccentricity > 1 ? new Vector3D(1, 0, 0) : Vector3D.Zero;
        var orbitalPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu);
        return new OrbitParameters
        {
            Type = OrbitType.Elliptical,
            T = NaNFix(orbitalPeriod),
            SemiMajorAxis = NaNFix(semiMajorAxis),
            Eccentricity = NaNFix(eccentricity),
            Inclination = NaNFix(inclination),
            LongitudeOfAscendingNode = NaNFix(longitudeOfAscendingNodeRad),
            ArgumentOfPeriapsis = NaNFix(argumentOfPeriapsisRad),
            TrueAnomaly = NaNFix(trueAnomalyRad),
            MeanAnomaly = NaNFix(meanAnomaly),
            TimeOfPeriapsisPassage = NaNFix(timeOfPeriapsisPassage),
            GravitationalParameter = NaNFix(mu),
            EpochTime = DateTime.UtcNow,
            AsymptoteDirection = new Vector3D(NaNFix(asymptoteDirection.X), NaNFix(asymptoteDirection.Y), NaNFix(asymptoteDirection.Z))
        };
    }
    public static float Influence(Vector3D shipPosition, Vector3D bodyPosition, double mass)
    {
        var d = Vector3D.Distance(shipPosition, bodyPosition);
        return (float)(G * mass / (d * d));
    }

    public static OrbitParameters KeplarOrbit(Vector3D position, Vector3D velocity, double bodyMass, DateTime date = default)
    {
        if (date == default) date = DateTime.UtcNow;
        double mu = G * bodyMass;

        // Calculate specific angular momentum
        Vector3D h = Vector3D.Cross(position, velocity);
        double hMagnitude = h.Magnitude();

        // Calculate eccentricity vector
        double r = position.Magnitude();
        double v = velocity.Magnitude();
        double rDotV = Vector3D.Dot(position, velocity);

        Vector3D eVector = ((v * v - mu / r) * position - rDotV * velocity) / mu;
        double eccentricity = eVector.Magnitude();

        // Calculate specific orbital energy
        double orbitalEnergy = v * v / 2 - mu / r;

        // Determine the orbit type
        OrbitType orbitType;
        if (eccentricity < 1)
        {
            orbitType = OrbitType.Elliptical;
        }
        else if (eccentricity == 1)
        {
            orbitType = OrbitType.Parabolic;
        }
        else
        {
            orbitType = OrbitType.Hyperbolic;
        }
        double semiMajorAxis = orbitType == OrbitType.Elliptical
            ? -mu / (2 * orbitalEnergy)
            //PeA
            : mu / (2 * Math.Abs(orbitalEnergy));
        // Calculate inclination
        double inclination = Math.Acos(h.Z / hMagnitude);
        // Calculate longitude of the ascending node
        Vector3D n = new Vector3D(-h.Y, h.X, 0); // Node line
        double nMagnitude = n.Magnitude();
        double longitudeOfAscendingNode = Math.Acos(n.X / nMagnitude);
        if (n.Y < 0)
        {
            longitudeOfAscendingNode = 2 * Math.PI - longitudeOfAscendingNode;
        }
        // Calculate argument of periapsis
        double argumentOfPeriapsis = Math.Acos(Vector3D.Dot(n, eVector) / (nMagnitude * eccentricity));
        if (eVector.Z < 0)
        {
            argumentOfPeriapsis = 2 * Math.PI - argumentOfPeriapsis;
        }
        // Calculate true anomaly
        double trueAnomaly = Math.Acos(Vector3D.Dot(eVector, position) / (eccentricity * r));
        if (rDotV < 0)
        {
            trueAnomaly = 2 * Math.PI - trueAnomaly;
        }
        // Calculate mean anomaly and time of periapsis passage
        double meanAnomaly = trueAnomaly - eccentricity * Math.Sin(trueAnomaly);
        double timeOfPeriapsisPassage = meanAnomaly / Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3));
        // For hyperbolic orbits, calculate the asymptote direction
        Vector3D asymptoteDirection = orbitType == OrbitType.Hyperbolic ? eVector.Normalize() : Vector3D.Zero;
        var orbitalPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3)/mu);
        return new OrbitParameters
        {
            Type = orbitType,
            T = NaNFix(orbitalPeriod),
            SemiMajorAxis = NaNFix(semiMajorAxis),
            Eccentricity = NaNFix(eccentricity),
            Inclination = NaNFix(inclination),
            LongitudeOfAscendingNode = NaNFix(longitudeOfAscendingNode),
            ArgumentOfPeriapsis = NaNFix(argumentOfPeriapsis),
            TrueAnomaly = NaNFix(trueAnomaly),
            MeanAnomaly = NaNFix(meanAnomaly),
            TimeOfPeriapsisPassage = NaNFix(timeOfPeriapsisPassage),
            GravitationalParameter = NaNFix(mu),
            EpochTime = date,
            AsymptoteDirection = new Vector3D(NaNFix(asymptoteDirection.X), NaNFix(asymptoteDirection.Y), NaNFix(asymptoteDirection.Z))
        };
    }
    public static Vector3D VelocityAtTime(this OrbitParameters parameters, DateTime time)
    {
        return parameters.PositionAtTime(time.AddSeconds(1d)) - parameters.PositionAtTime(time);
    }
    static double NaNFix(double v) => double.IsNaN(v) ? 0f : v;
    public static double OrbitVelocity(double radius, double planetMass)
    {
        double velocity = Math.Sqrt(G * planetMass / radius);
        return (double)velocity;
    }
    public static IEnumerable<Vector3D> OrbitPoints(OrbitParameters p, int points)
    {
        List<Vector3D> coordinates = new List<Vector3D>();

        // Pre-compute the rotation matrix components
        double cosInclination = Math.Cos(p.Inclination);
        double sinInclination = Math.Sin(p.Inclination);
        double cosLongitudeOfAscendingNode = Math.Cos(p.LongitudeOfAscendingNode);
        double sinLongitudeOfAscendingNode = Math.Sin(p.LongitudeOfAscendingNode);
        double cosArgumentOfPeriapsis = Math.Cos(p.ArgumentOfPeriapsis);
        double sinArgumentOfPeriapsis = Math.Sin(p.ArgumentOfPeriapsis);
        // Define the range for the true anomaly for hyperbolic orbits, e.g., from -π/2 to π/2 radians
        double startAnomaly = 0f;
        double endAnomaly = Math.PI * 2;
        if (p.Type == OrbitType.Hyperbolic)
        {
            // Use a higher sampling range for hyperbolic orbits to capture more of the trajectory
            startAnomaly = 1f - -Math.PI / 2f; // Adjust according to desired segment
            endAnomaly = 1f - Math.PI / 2f; // Adjust according to desired segment
        }
        // Loop through the points
        for (int i = 0; i < points; i++)
        {
            double trueAnomaly = startAnomaly + (endAnomaly - startAnomaly) * i / points;

            // Calculate the radius in the orbital plane
            double radius = p.SemiMajorAxis * (1 - p.Eccentricity * p.Eccentricity) / (1 + p.Eccentricity * Math.Cos(trueAnomaly));
            // Position in the orbital plane
            double xOrbital = radius * Math.Cos(trueAnomaly);
            double yOrbital = radius * Math.Sin(trueAnomaly);
            // Transform the coordinates to the inertial frame
            double xInertial, yInertial, zInertial;
            if (p.Eccentricity == 0f)
            {
                xInertial = xOrbital * cosLongitudeOfAscendingNode -
                            yOrbital * sinLongitudeOfAscendingNode * cosInclination;
                yInertial = xOrbital * sinLongitudeOfAscendingNode +
                            yOrbital * cosLongitudeOfAscendingNode * cosInclination;
                zInertial = yOrbital * sinInclination;
            }
            else
            {
                xInertial = (cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis - sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                                   (-cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis - sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
                yInertial = (sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis + cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                                   (-sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis + cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
                zInertial = sinArgumentOfPeriapsis * sinInclination * xOrbital +
                                   cosArgumentOfPeriapsis * sinInclination * yOrbital;
            }
            // Add the point to the list
            yield return new Vector3D((double)xInertial, (double)yInertial, (double)zInertial);
        }
    }
    public static Vector3D PositionAtTime(this OrbitParameters parameters, DateTime time)
    {
        // Calculate the mean anomaly at the given time
        double meanMotion = Math.Sqrt(parameters.GravitationalParameter / Math.Pow(parameters.SemiMajorAxis, 3));
        double timeSincePeriapsis = parameters.TimeOfPeriapsisPassage - (double)(time - parameters.EpochTime).TotalSeconds;
        double meanAnomaly = parameters.MeanAnomaly + meanMotion * timeSincePeriapsis;

        // Solve Kepler's equation to get the eccentric anomaly
        double eccentricAnomaly = meanAnomaly;
        for (int i = 0; i < 10; i++) // Iterate to solve Kepler's equation
        {
            eccentricAnomaly = meanAnomaly + parameters.Eccentricity * Math.Sin(eccentricAnomaly);
        }

        // Calculate the true anomaly from the eccentric anomaly
        double trueAnomaly = 2 * Math.Atan2(
            Math.Sqrt(1 + parameters.Eccentricity) * Math.Sin(eccentricAnomaly / 2),
            Math.Sqrt(1 - parameters.Eccentricity) * Math.Cos(eccentricAnomaly / 2)
        );

        // Calculate the radius in the orbital plane
        double radius = parameters.SemiMajorAxis * (1 - parameters.Eccentricity * Math.Cos(eccentricAnomaly));

        // Position in the orbital plane
        double xOrbital = radius * Math.Cos(trueAnomaly);
        double yOrbital = radius * Math.Sin(trueAnomaly);

        // Transform the coordinates to the inertial frame
        double cosInclination = Math.Cos(parameters.Inclination);
        double sinInclination = Math.Sin(parameters.Inclination);
        double cosLongitudeOfAscendingNode = Math.Cos(parameters.LongitudeOfAscendingNode);
        double sinLongitudeOfAscendingNode = Math.Sin(parameters.LongitudeOfAscendingNode);
        double cosArgumentOfPeriapsis = Math.Cos(parameters.ArgumentOfPeriapsis);
        double sinArgumentOfPeriapsis = Math.Sin(parameters.ArgumentOfPeriapsis);

        double xInertial = (cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis - sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                          (-cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis - sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
        double yInertial = (sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis + cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                          (-sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis + cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
        double zInertial = sinArgumentOfPeriapsis * sinInclination * xOrbital +
                          cosArgumentOfPeriapsis * sinInclination * yOrbital;

        return new Vector3D(xInertial, yInertial, zInertial);
    }
    public static (Vector3D position, Vector3D velocity) ApplyGravity(Vector3D position, Vector3D velocity, Vector3D planetPosition, double planetMass, double stepTimeSeconds)
    {
        double massOfObject = 1.0f; // Assuming a unit mass for the object since mass cancels out in F = ma for gravitational calculations.

        // Calculate the gravitational force
        Vector3D relativePosition = position - planetPosition;
        double distance = relativePosition.Length();
        double forceMagnitude = G * planetMass * massOfObject / (distance * distance);

        // Compute the direction of the force
        Vector3D forceDirection = relativePosition.Normalize() * -1;

        // Calculate the acceleration (Newton's Second Law: F = ma -> a = F / m)
        Vector3D acceleration = forceDirection * forceMagnitude / massOfObject;

        // Update velocity based on acceleration (v = u + at)
        Vector3D newVelocity = velocity + acceleration * stepTimeSeconds;

        // Update position based on new velocity (s = ut + 0.5at^2 can be simplified in this step-wise approach to s = s + vt)
        Vector3D newPosition = position + newVelocity * stepTimeSeconds;

        return (newPosition, newVelocity);
    }
}

