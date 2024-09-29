public class Simulation
{
    public DateTime SimulationTime { get; set; } = default;
    List<CelestialBody> celestialBodies = new List<CelestialBody>();
    public IEnumerable<CelestialBody> CelestialBodies => celestialBodies;
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
            var distance = Vector3D.Distance(camera.Position, position);
            if (distance < 1000 - body.Size)
            {
                DrawModel(body.Model.Value, position, (float)body.Size, Color.White);
            }
        }
    }
    public void DrawFarAwayBodies(Camera3D camera)
    {
        foreach (var body in celestialBodies)
        {
            var position = body.GetPosition(SimulationTime);
            var distance = Vector3D.Distance(camera.Position, position);
            if (body.Model == null || distance >= 1000 - body.Size)
            {
                var screenPosition = GetWorldToScreen(position, camera);
                if (screenPosition.X >= 0 && screenPosition.X <= GetScreenWidth() && screenPosition.Y >= 0 && screenPosition.Y <= GetScreenHeight())
                {
                    double sizeFactor = 1000 / distance;
                    float drawSize = (float)Math.Max(1f, body.Size * sizeFactor);
                    DrawCircle((int)double.Round(screenPosition.X), (int)double.Round(screenPosition.Y), drawSize, body.FarColor);
                }
            }
        }
    }
    public void DrawOrbits2D(Camera3D camera, CelestialBody? centerBody = null)
    {
        foreach (var body in celestialBodies)
        {
            if (body.OrbitPoints != null)
            {
                var color = (centerBody != null && centerBody == body) ? new Color(40,40,40,255) : Color.DarkGray;
                Drawing.Draw2DLineOfPoints(camera, body.OrbitPoints!.Select(p => p + body.CentralBody!.GetPosition(SimulationTime)).ToArray(), color);
            }
        }
    }
}