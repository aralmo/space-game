public class Simulation
{
    public DateTime SimulationTime { get; set; } = default;
    List<CelestialBody> celestialBodies = new List<CelestialBody>();
    public Simulation AddCelestialBody(CelestialBody body)
    {
        celestialBodies.Add(body);
        return this;
    }
    
    //todo: everything below this point should go to a SimulationVisuals of some kind

    public void Draw(Camera3D camera)
    {
        foreach (var body in celestialBodies)
        {
            if (body.Model == null) continue;
            var position = body.GetPosition(SimulationTime);
            var distance = Vector3.Distance(camera.Position, position);
            if (distance < 1000 - body.Size)
            {
                DrawModel(body.Model.Value, position, body.Size, Color.White);
            }
        }
    }
    public void DrawFarAwayBodies(Camera3D camera)
    {
        foreach (var body in celestialBodies)
        {
            var position = body.GetPosition(SimulationTime);
            var distance = Vector3.Distance(camera.Position, position);
            if (body.Model == null ||  distance >= 1000 - body.Size)
            {
                var screenPosition = GetWorldToScreen(position, camera);
                if (screenPosition.X >= 0 && screenPosition.X <= GetScreenWidth() && screenPosition.Y >= 0 && screenPosition.Y <= GetScreenHeight())
                {
                    float sizeFactor = 1000 / distance; // Adjust size based on distance
                    float drawSize = body.Size * sizeFactor;
                    DrawCircle((int) float.Round(screenPosition.X), (int)float.Round(screenPosition.Y), drawSize, body.FarColor);
                }
            }
        }
    }
    public void DrawOrbits2D(Camera3D camera, CelestialBody? centerBody = null)
    {
        foreach (var body in celestialBodies)
        {
            if ((centerBody == null || centerBody.Equals(body.CentralBody)) && body.OrbitPoints != null && body.CentralBody != null)
            {
                Drawing.Draw2DLineOfPoints(camera, body.OrbitPoints.Select(p => p+ body.CentralBody.GetPosition(SimulationTime)).ToArray());
            }
        }
    }
}