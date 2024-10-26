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
        UpdateShipsPositions();
    }
    private static unsafe void UpdateShipsPositions()
    {
        foreach (var ship in Game.Spaceships)
        {
            if (ship.Prediction == null) return;
            var shipPrediction = ship.Prediction;
            PredictedPoint? currentShipPoint = shipPrediction.Points.FirstOrDefault(t => t.Time >= Game.Simulation.Time);
            if (currentShipPoint != null)
            {
                ship.Simulation.Position = currentShipPoint.Position;
                ship.Simulation.Velocity = currentShipPoint.Velocity;
                ship.Simulation.MajorInfluenceBody = currentShipPoint.MajorInfluence;
                //ship.EnginePlaying = currentShipPoint.TimeAccelerating > 0;
            }
            else
            {
                var lastPoint = ship.Prediction?.Points?.LastOrDefault();
                if (lastPoint?.IsJoin ?? false)
                {
                    //Game.PlayerShip.Stationed = lastPoint.JoinObject!;
                }

                Game.Simulation.Speed = 0;
                shipPrediction.Reset();
            }
        }
    }



}