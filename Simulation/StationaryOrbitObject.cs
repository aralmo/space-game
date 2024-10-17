

/// <summary>
/// An object that is stationary in orbit of a celestial body, like a station or an artificial satellite.
/// Stationary means it doesn't change orbit, so it's keplar based, but it's orbiting, so it moves.
/// </summary>
public class StationaryOrbitObject : OrbitingObject
{
    public StationaryOrbitObject(Func<DateTime, Vector3D> positionFunction) : base(positionFunction, 0)
    {
    }

    /// <summary>
    /// Creates a stationary orbit object with a specified mass, orbiting around a celestial body.
    /// </summary>
    /// <param name="centralBody">The celestial body around which the object orbits.</param>
    /// <param name="parameters">The parameters defining the orbit.</param>
    /// <param name="mass">The mass of the orbiting object.</param>
    /// <returns>A new StationaryOrbitObject instance.</returns>
    public static StationaryOrbitObject CreateStationary(OrbitingObject centralBody, OrbitParameters parameters)
    {
        var stationary = new StationaryOrbitObject(time => parameters.PositionAtTime(time))
        {
            OrbitParameters = parameters,
            CentralBody = centralBody,
            OrbitPoints = (parameters.Type == OrbitType.Elliptical)
                ? Solve.OrbitPoints(parameters, (int)parameters.SemiMajorAxis * 2).ToArray()
                : null,
        };

        return stationary;
    }

    public void Draw3D(DateTime? time)
    {
        var d = time.HasValue ? time.Value : Game.Simulation.Time;
        DrawCube(GetPosition(d), 1, 2, 1, Color.Violet);
    }
}