public class PlayTurnView : GameView
{
    public override bool Running => Game.Simulation?.Speed > 0;
    public override void Draw2D()
    {
        base.Draw2D();
        PlanningView.DrawPredictedManeuver(allowControl: false);
        Game.Simulation.DrawOrbits2D(Camera.Current);
    }

    public override void Draw2DAfter()
    {
        base.Draw2DAfter();
        DrawUI.SimSpeedControls(10, 10);
    }
    public override void Update()
    {
        base.Update();
        UpdateShipPosition();
    }
    private static unsafe void UpdateShipPosition()
    {
        if (Game.PlayerShip?.Prediction == null) return;
        var shipPrediction = Game.PlayerShip.Prediction;
        PredictedPoint? currentShipPoint = shipPrediction.Points.FirstOrDefault(t => t.Time >= Game.Simulation.Time);
        if (currentShipPoint != null)
        {
            Game.PlayerShip.DynamicSimulation.Position = currentShipPoint.Position;
            Game.PlayerShip.DynamicSimulation.Velocity = currentShipPoint.Velocity;
            Game.PlayerShip.DynamicSimulation.MajorInfluenceBody = currentShipPoint.MajorInfluence;
            Game.PlayerShip.EnginePlaying = currentShipPoint.TimeAccelerating > 0;
        }
        else
        {
            var lastPoint = Game.PlayerShip?.Prediction?.Points?.LastOrDefault();
            if (lastPoint?.IsJoin ?? false)
            {
                Game.PlayerShip.Stationed = lastPoint.JoinObject!;
            }

            Game.Simulation.Speed = 0;
            shipPrediction.Reset();
        }
    }



}