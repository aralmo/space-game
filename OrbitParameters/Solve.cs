using static Constants;

public static class Solve
{
    public static OrbitParameters OrbitFor(Vector3 position, Vector3 velocity, float bodyMass, DateTime date = default)
    {
        if (date == default) date = DateTime.UtcNow;
        float mu = G * bodyMass;

        // Calculate specific angular momentum
        Vector3 h = Vector3.Cross(position, velocity);
        float hMagnitude = h.Magnitude();

        // Calculate eccentricity vector
        float r = position.Magnitude();
        float v = velocity.Magnitude();
        float rDotV = Vector3.Dot(position, velocity);

        Vector3 eVector = ((v * v - mu / r) * position - rDotV * velocity) / mu;
        float eccentricity = eVector.Magnitude();

        // Calculate specific orbital energy
        float orbitalEnergy = v * v / 2 - mu / r;

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
        float semiMajorAxis = orbitType == OrbitType.Elliptical ? -mu / (2 * orbitalEnergy) : float.PositiveInfinity;
        // Calculate inclination
        float inclination = MathF.Acos(h.Z / hMagnitude);
        // Calculate longitude of the ascending node
        Vector3 n = new Vector3(-h.Y, h.X, 0); // Node line
        float nMagnitude = n.Magnitude();
        float longitudeOfAscendingNode = MathF.Acos(n.X / nMagnitude);
        if (n.Y < 0)
        {
            longitudeOfAscendingNode = 2 * MathF.PI - longitudeOfAscendingNode;
        }
        // Calculate argument of periapsis
        float argumentOfPeriapsis = MathF.Acos(Vector3.Dot(n, eVector) / (nMagnitude * eccentricity));
        if (eVector.Z < 0)
        {
            argumentOfPeriapsis = 2 * MathF.PI - argumentOfPeriapsis;
        }
        // Calculate true anomaly
        float trueAnomaly = MathF.Acos(Vector3.Dot(eVector, position) / (eccentricity * r));
        if (rDotV < 0)
        {
            trueAnomaly = 2 * MathF.PI - trueAnomaly;
        }
        // Calculate mean anomaly and time of periapsis passage
        float meanAnomaly = trueAnomaly - eccentricity * MathF.Sin(trueAnomaly);
        float timeOfPeriapsisPassage = meanAnomaly / MathF.Sqrt(mu / MathF.Pow(semiMajorAxis, 3));
        // For hyperbolic orbits, calculate the asymptote direction
        Vector3 asymptoteDirection = orbitType == OrbitType.Hyperbolic ? eVector.Normalized() : Vector3.Zero;
        var orbitalPeriod = 2 * MathF.PI * MathF.Sqrt(MathF.Pow(semiMajorAxis, 3) / mu);
        return new OrbitParameters
        {
            Type = orbitType,
            T = orbitalPeriod,
            SemiMajorAxis = semiMajorAxis,
            Eccentricity = eccentricity,
            Inclination = inclination,
            LongitudeOfAscendingNode = longitudeOfAscendingNode,
            ArgumentOfPeriapsis = argumentOfPeriapsis,
            TrueAnomaly = trueAnomaly,
            MeanAnomaly = meanAnomaly,
            TimeOfPeriapsisPassage = timeOfPeriapsisPassage,
            GravitationalParameter = mu,
            EpochTime = date,
            AsymptoteDirection = asymptoteDirection
        };
    }
    public static float OrbitVelocity(float radius, float planetMass)
    {
        float velocity = MathF.Sqrt(G * planetMass / radius);
        return (float)velocity;
    }
    public static IEnumerable<Vector3> OrbitCartesianPoints(OrbitParameters p, int points)
    {
        List<Vector3> coordinates = new List<Vector3>();
        
        // Pre-compute the rotation matrix components
        float cosInclination = MathF.Cos(p.Inclination);
        float sinInclination = MathF.Sin(p.Inclination);
        float cosLongitudeOfAscendingNode = MathF.Cos(p.LongitudeOfAscendingNode);
        float sinLongitudeOfAscendingNode = MathF.Sin(p.LongitudeOfAscendingNode);
        float cosArgumentOfPeriapsis = MathF.Cos(p.ArgumentOfPeriapsis);
        float sinArgumentOfPeriapsis = MathF.Sin(p.ArgumentOfPeriapsis);

        // Loop through the points
        for (int i = 0; i < points; i++)
        {
            float trueAnomaly = 2 * MathF.PI * i / points;

            // Calculate the radius in the orbital plane
            float radius = p.SemiMajorAxis * (1 - p.Eccentricity * p.Eccentricity) / (1 + p.Eccentricity * MathF.Cos(trueAnomaly));
            // Position in the orbital plane
            float xOrbital = radius * MathF.Cos(trueAnomaly);
            float yOrbital = radius * MathF.Sin(trueAnomaly);
            // Transform the coordinates to the inertial frame
            float xInertial = (cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis - sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                               (-cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis - sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
            float yInertial = (sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis + cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                               (-sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis + cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
            float zInertial = sinArgumentOfPeriapsis * sinInclination * xOrbital +
                               cosArgumentOfPeriapsis * sinInclination * yOrbital;
            // Add the point to the list
            yield return new Vector3((float)xInertial, (float)yInertial, (float)zInertial);
        }
    }
}