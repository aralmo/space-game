public class PlayerShip
{
    private readonly Model model;

    public Simulation Simulation { get; private set; }
    public DynamicSimulation DynamicSimulation { get; private set; }

    public PlayerShip(Simulation simulation, DynamicSimulation dynamicSimulation, Model model)
    {
        Simulation = simulation;
        this.model = model;
        DynamicSimulation = dynamicSimulation;
    }

    public void Update()
    {
        Simulation.Update();
    }

    public void Draw3D()
    {
        DynamicSimulation.Draw3D(model);
    }
}
