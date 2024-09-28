public class Simulation
{
    public DateTime SimulationTime {get;set;} = default;
    List<CelestialBody> celestialBodies = new List<CelestialBody>();
    public Simulation AddCelestialBody(CelestialBody body)
    {
        celestialBodies.Add(body);
        return this;
    }

    public void Draw(bool drawOrbit = false)
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
            if (drawOrbit && body.OrbitPoints != null)
            {
                DrawLineOfPoints(body.OrbitPoints!, Color.Gray);
            }
        }
    }
}