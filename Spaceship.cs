public class Spaceship
{
    public int Owner;
    public ShipModel Model;
    public DynamicSimulation Simulation;
    public PathPrediction Prediction;

    public float DeltaV = 1000;

    public void Draw3D()
    {
        var fwd = Simulation.ForwardRotation();
        Model.RotationAxis = fwd.axis;
        Model.Rotation = fwd.rotation;
        Model.Position = Simulation.Position;
        var accelerating = Prediction.Points?.FirstOrDefault(p => p.Time >= Game.Simulation.Time)?.TimeAccelerating > 0;
        foreach(Transform tr in Model.Transforms.Where(t => t is EngineEmitter))
        {
            tr.Enabled = accelerating;
        }
        Model.Draw3D();
    }
    public void Update()
    {

        Prediction.Update();
        Model.Update();
    }
}
