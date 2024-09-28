using static Constants;

public static class Solve
{
    public static OrbitParameters KeplarOrbit(Vector3 position, Vector3 velocity, float bodyMass, DateTime date = default)
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
        float semiMajorAxis = orbitType == OrbitType.Elliptical
            ? -mu / (2 * orbitalEnergy)
            //PeA
            : mu / (2 * MathF.Abs(orbitalEnergy));
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
    public static IEnumerable<Vector3> OrbitPoints(OrbitParameters p, int points)
    {
        List<Vector3> coordinates = new List<Vector3>();

        // Pre-compute the rotation matrix components
        float cosInclination = MathF.Cos(p.Inclination);
        float sinInclination = MathF.Sin(p.Inclination);
        float cosLongitudeOfAscendingNode = MathF.Cos(p.LongitudeOfAscendingNode);
        float sinLongitudeOfAscendingNode = MathF.Sin(p.LongitudeOfAscendingNode);
        float cosArgumentOfPeriapsis = MathF.Cos(p.ArgumentOfPeriapsis);
        float sinArgumentOfPeriapsis = MathF.Sin(p.ArgumentOfPeriapsis);
        // Define the range for the true anomaly for hyperbolic orbits, e.g., from -π/2 to π/2 radians
        float startAnomaly = 0f;
        float endAnomaly = MathF.PI * 2;
        if (p.Type == OrbitType.Hyperbolic)
        {
            // Use a higher sampling range for hyperbolic orbits to capture more of the trajectory
            startAnomaly =1f- -MathF.PI /2f; // Adjust according to desired segment
            endAnomaly =1f- MathF.PI /2f; // Adjust according to desired segment
        }
        // Loop through the points
        for (int i = 0; i < points; i++)
        {
            float trueAnomaly = startAnomaly + (endAnomaly - startAnomaly) * i / points;

            // Calculate the radius in the orbital plane
            float radius = p.SemiMajorAxis * (1 - p.Eccentricity * p.Eccentricity) / (1 + p.Eccentricity * MathF.Cos(trueAnomaly));
            // Position in the orbital plane
            float xOrbital = radius * MathF.Cos(trueAnomaly);
            float yOrbital = radius * MathF.Sin(trueAnomaly);
            // Transform the coordinates to the inertial frame
            float xInertial, yInertial, zInertial;
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
            yield return new Vector3((float)xInertial, (float)yInertial, (float)zInertial);
        }
    }
    public static Vector3 PositionAtTime(this OrbitParameters parameters, DateTime time)
    {
        // Calculate the mean anomaly at the given time
        float meanMotion = MathF.Sqrt(parameters.GravitationalParameter / MathF.Pow(parameters.SemiMajorAxis, 3));
        float timeSincePeriapsis = parameters.TimeOfPeriapsisPassage - (float)(time - parameters.EpochTime).TotalSeconds;
        float meanAnomaly = parameters.MeanAnomaly + meanMotion * timeSincePeriapsis;

        // Solve Kepler's equation to get the eccentric anomaly
        float eccentricAnomaly = meanAnomaly;
        for (int i = 0; i < 10; i++) // Iterate to solve Kepler's equation
        {
            eccentricAnomaly = meanAnomaly + parameters.Eccentricity * MathF.Sin(eccentricAnomaly);
        }

        // Calculate the true anomaly from the eccentric anomaly
        float trueAnomaly = 2 * MathF.Atan2(
            MathF.Sqrt(1 + parameters.Eccentricity) * MathF.Sin(eccentricAnomaly / 2),
            MathF.Sqrt(1 - parameters.Eccentricity) * MathF.Cos(eccentricAnomaly / 2)
        );

        // Calculate the radius in the orbital plane
        float radius = parameters.SemiMajorAxis * (1 - parameters.Eccentricity * MathF.Cos(eccentricAnomaly));

        // Position in the orbital plane
        float xOrbital = radius * MathF.Cos(trueAnomaly);
        float yOrbital = radius * MathF.Sin(trueAnomaly);

        // Transform the coordinates to the inertial frame
        float cosInclination = MathF.Cos(parameters.Inclination);
        float sinInclination = MathF.Sin(parameters.Inclination);
        float cosLongitudeOfAscendingNode = MathF.Cos(parameters.LongitudeOfAscendingNode);
        float sinLongitudeOfAscendingNode = MathF.Sin(parameters.LongitudeOfAscendingNode);
        float cosArgumentOfPeriapsis = MathF.Cos(parameters.ArgumentOfPeriapsis);
        float sinArgumentOfPeriapsis = MathF.Sin(parameters.ArgumentOfPeriapsis);

        float xInertial = (cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis - sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                          (-cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis - sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
        float yInertial = (sinLongitudeOfAscendingNode * cosArgumentOfPeriapsis + cosLongitudeOfAscendingNode * sinArgumentOfPeriapsis * cosInclination) * xOrbital +
                          (-sinLongitudeOfAscendingNode * sinArgumentOfPeriapsis + cosLongitudeOfAscendingNode * cosArgumentOfPeriapsis * cosInclination) * yOrbital;
        float zInertial = sinArgumentOfPeriapsis * sinInclination * xOrbital +
                          cosArgumentOfPeriapsis * sinInclination * yOrbital;

        return new Vector3(xInertial, yInertial, zInertial);
    }
    public static (Vector3 position, Vector3 velocity) ApplyGravity(Vector3 position, Vector3 velocity, float planetMass, float stepTimeSeconds)
    {
        float massOfObject = 1.0f; // Assuming a unit mass for the object since mass cancels out in F = ma for gravitational calculations.

        // Calculate the gravitational force
        float distance = position.Length();
        float forceMagnitude = G * planetMass * massOfObject / (distance * distance);

        // Compute the direction of the force
        Vector3 forceDirection = Vector3.Normalize(-position);

        // Calculate the acceleration (Newton's Second Law: F = ma -> a = F / m)
        Vector3 acceleration = forceDirection * forceMagnitude / massOfObject;

        // Update velocity based on acceleration (v = u + at)
        Vector3 newVelocity = velocity + acceleration * stepTimeSeconds;

        // Update position based on new velocity (s = ut + 0.5at^2 can be simplified in this step-wise approach to s = s + vt)
        Vector3 newPosition = position + newVelocity * stepTimeSeconds;

        return (newPosition, newVelocity);
    }
}

