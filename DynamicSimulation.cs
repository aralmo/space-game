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

    public (IEnumerable<Vector3D> orbit, OrbitingObject? plane) PredictedPath(Simulation sim, int seconds = 120, int fps = 60, int maxIterations = 60000)
    {
        float step = 1f;
        if (seconds * fps > maxIterations){
            step = (float) (seconds * fps) / maxIterations;
        }

        var predictedPath = new List<Vector3D>();
        var tempPosition = Position;
        var tempVelocity = Velocity;
        var tempSimulationTime = sim.SimulationTime;

        CelestialBody? planeOfReference = null;
        double highestInfluence = double.MinValue;

        foreach (var obj in sim.OrbitingBodies)
        {
            if (obj is CelestialBody body)
            {
                var bodyPosition = body.GetPosition(sim.SimulationTime);
                var distance = Vector3.Distance(Position, bodyPosition);
                var influence = Constants.G * body.Mass / (distance * distance);
                if (influence < Constants.MIN_INFLUENCE) continue;
                if (influence > highestInfluence)
                {
                    highestInfluence = influence;
                    planeOfReference = body;
                }
            }
        }
        for (float t = 0f; t < seconds * fps; t += step){
            // Update the temporary simulation time

            // Apply gravity to update velocity
            foreach (var obj in sim.OrbitingBodies)
            {
                if (obj is CelestialBody body)
                {
                    var bodyPosition = body.GetPosition(tempSimulationTime);
                    var distance = Vector3D.Distance(tempPosition, bodyPosition);
                    var influence = G * body.Mass / (distance * distance);
                    if (influence < MIN_INFLUENCE) continue;

                    // Calculate the gravitational force
                    var direction = (bodyPosition - tempPosition).Normalize();
                    var force = direction * influence;

                    // Update the velocity based on the force
                    tempVelocity += force * (step / fps);
                }
            }

            // Update position based on the new velocity
            tempPosition += tempVelocity * (step / fps);
            tempSimulationTime = tempSimulationTime.AddSeconds(step / fps);

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

        return (predictedPath, planeOfReference);
    }
}