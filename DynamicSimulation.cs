public class DynamicSimulation
{
    public Vector3D Position { get; set; }
    public Vector3D Velocity { get; set; }
    public DynamicSimulation(Vector3D position, Vector3D velocity)
    {
        Position = position;
        Velocity = velocity;
    }
    public void ApplyGravity(DateTime? time, Simulation simulation, float deltaTime)
    {
        foreach (var obj in simulation.OrbitingBodies)
        {
            if (obj is CelestialBody body)
            {
                var bodyPosition = body.GetPosition(time ?? simulation.SimulationTime);
                var distance = Vector3.Distance(Position, bodyPosition);
                var influence = G * body.Mass / (distance * distance);
                if (influence < MIN_INFLUENCE) continue;
                (_, Velocity) = Solve.ApplyGravity(
                    position: Position,
                    velocity: Velocity,
                    planetPosition: body.GetPosition(time ?? simulation.SimulationTime),
                    planetMass: body.Mass,
                    stepTimeSeconds: deltaTime);
            }
        }
    }
    public void UpdatePosition(float deltaTime)
    {
        Position += Velocity * deltaTime;
    }

    public IEnumerable<Vector3D> PredictedPath(Simulation sim, int seconds = 120, int fps = 60, OrbitingObject? planeOfReference = null)
    {
        var predictedPath = new List<Vector3D>();
        var tempPosition = Position;
        var tempVelocity = Velocity;
        var tempSimulationTime = sim.SimulationTime;

        for (int i = 0; i < seconds * fps; i++)
        {
            // Update the temporary simulation time

            // Apply gravity to update velocity
            foreach (var obj in sim.OrbitingBodies)
            {
                if (obj is CelestialBody body)
                {
                    var bodyPosition = body.GetPosition(tempSimulationTime);
                    var distance = Vector3.Distance(tempPosition, bodyPosition);
                    var influence = G * body.Mass / (distance * distance);
                    if (influence < MIN_INFLUENCE) continue;
                    (_, tempVelocity) = Solve.ApplyGravity(
                        position: tempPosition,
                        velocity: tempVelocity,
                        planetPosition: body.GetPosition(tempSimulationTime),
                        planetMass: body.Mass,
                        stepTimeSeconds: 1.0f / fps);
                }
            }

            // Update position based on the new velocity
            tempPosition += tempVelocity * (1.0 / fps);
            tempSimulationTime = tempSimulationTime.AddSeconds(1.0 / fps);

            // Add the new position to the predicted path
            if (planeOfReference != null)
            {
                var referencePosition = planeOfReference.GetPosition(tempSimulationTime);
                var vector = tempPosition - referencePosition;
                predictedPath.Add(vector);
            }
            else
            {
                predictedPath.Add(tempPosition);
            }
        }

        return predictedPath;
    }
}