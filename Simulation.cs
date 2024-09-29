public class Simulation
{
    public DateTime SimulationTime { get; set; } = default;
    List<CelestialBody> celestialBodies = new List<CelestialBody>();
    public Simulation AddCelestialBody(CelestialBody body)
    {
        celestialBodies.Add(body);
        return this;
    }

    public void Draw(bool drawOrbits = false)
    {
        foreach (var body in celestialBodies)
        {
            if (body.Model != null)
            {
                DrawModel(body.Model.Value, body.GetPosition(SimulationTime), body.Size, Color.White);
            }
            else
            {
                DrawSphere(body.GetPosition(SimulationTime), body.Size, Color.White);
            }

            if (drawOrbits && body.OrbitPoints != null && body.CentralBody != null)
            {
                DrawLineOfPoints(body.OrbitPoints!.Select(p => p + body.CentralBody.GetPosition(SimulationTime)), Color.Gray);
            }
        }
    }
    public void DrawOrbits2D(Camera3D camera)
    {
        foreach (var body in celestialBodies)
        {
            if (body.OrbitPoints != null && body.CentralBody != null)
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
                    DrawLine((int)orbitPoints2D[i].X, (int)orbitPoints2D[i].Y, (int)orbitPoints2D[i + 1].X, (int)orbitPoints2D[i + 1].Y, Color.Gray);
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