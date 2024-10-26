public class Spaceship
{
    public int Owner;
    public ShipModel Model;
    public DynamicSimulation Simulation;
    public PathPrediction Prediction;

    public float DeltaV = 1000;

    public void Draw3D()
    {
        Model.Position = Simulation.Position;
        Model.Draw3D();
    }
    public void Update()
    {
        Prediction.Update();
        Model.Update();
    }
}
