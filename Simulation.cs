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
                var orbitPoints2D = body.OrbitPoints
                    .Select(p => GetWorldToScreen(p + body.CentralBody.GetPosition(SimulationTime), camera))
                    .ToList();

                for (int i = 0; i < orbitPoints2D.Count - 1; i++)
                {
                    bool anyPointInsideScreen = orbitPoints2D.Any(p => p.X >= 0 && p.X <= GetScreenWidth() && p.Y >= 0 && p.Y <= GetScreenHeight());
                    if (!anyPointInsideScreen)
                    {
                        continue;
                    }
                    DrawLine((int)float.Round(orbitPoints2D[i].X), (int)float.Round(orbitPoints2D[i].Y), (int)float.Round(orbitPoints2D[i + 1].X), (int)float.Round(orbitPoints2D[i + 1].Y), Color.Gray);
                }
                if (orbitPoints2D.Count > 1)
                {
                    bool anyPointInsideScreen = orbitPoints2D.Any(p => p.X >= 0 && p.X <= GetScreenWidth() && p.Y >= 0 && p.Y <= GetScreenHeight());
                    if (anyPointInsideScreen)
                    {
                        var firstPoint = orbitPoints2D.First();
                        var lastPoint = orbitPoints2D.Last();
                        DrawLine((int)lastPoint.X, (int)lastPoint.Y, (int)firstPoint.X, (int)firstPoint.Y, Color.Gray);
                    }
                }
            }
        }
    }
}