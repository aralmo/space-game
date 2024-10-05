public class Simulation
{
    int Speed = 1;
    public DateTime SimulationTime;
    List<OrbitingObject> orbitingBodies = new List<OrbitingObject>();
    public IEnumerable<OrbitingObject> OrbitingBodies => orbitingBodies;
    public Simulation AddOrbitingBody(OrbitingObject body)
    {
        orbitingBodies.Add(body);
        return this;
    }
    List<DynamicSimulation> dynamicSimulations = new();
    public Simulation RegisterDynamicForUpdate(DynamicSimulation simulation)
    {
        dynamicSimulations.Add(simulation);
        return this;
    }
    public void Update()
    {
        float deltaTime = 1.0f / TARGET_FPS;
        for (int i = 0; i < Speed; i++)
        {
            SimulationTime = SimulationTime.AddSeconds(deltaTime);
            foreach (var ds in dynamicSimulations)
            {
                ds.ApplyGravity(SimulationTime, deltaTime);
                ds.UpdatePosition(deltaTime);
            }
        }
    }










    public void Draw3D(Camera3D camera)
    {
        foreach (var obj in orbitingBodies)
        {
            if (obj is CelestialBody body)
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
    }
    public void Draw2D(Camera3D camera)
    {
        foreach (var obj in orbitingBodies)
        {
            if (obj is CelestialBody body)
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
    }
    public void DrawOrbits2D(Camera3D camera, OrbitingObject? centerBody = null)
    {
        foreach (var body in orbitingBodies)
        {
            if (body.OrbitPoints != null)
            {
                var color = (centerBody != null && centerBody == body) ? new Color(40, 40, 40, 255) : Color.DarkGray;
                Drawing.Draw2DLineOfPoints(camera, body.OrbitPoints!.Select(p => p + body.CentralBody!.GetPosition(SimulationTime)).ToArray(), color);
            }
        }
    }
}