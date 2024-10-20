public class PlanningView : GameView
{
    public override bool Running => Game.Simulation.Speed == 0;
    public override void Update()
    {
        base.Update();
        ManeuverControls();
    }
    public override void Draw2D()
    {
        Game.Simulation.DrawOrbits2D(Camera.Current, out Vector3D? _);
    }
    public override void Draw2DAfter()
    {
        base.Draw2D();
        DrawPredictedManeuver();
        DrawUI.SimSpeedControls(10, 10);
    }
    internal static unsafe void DrawPredictedManeuver(bool allowControl = true)
    {
        var shipPrediction = Game.PlayerShip.Prediction;
        var predictionDisplay = shipPrediction.Points.Where(p => p.Time >= Game.Simulation.Time).Decimate(400).ToArray();
        var mouse_position = GetMousePosition();
        float distanceToMouse = float.MaxValue;
        PredictedPoint closest = default;
        Vector2 closestViewPos = default;
        float dtomouse;
        bool mouse_control = allowControl;
        //Draw the 2D line for the ship's current predicted path
        for (int i = 0; i < predictionDisplay.Length - 1; i++)
        {
            var prA = RelativePredictedPoint(predictionDisplay[i]);
            var prB = RelativePredictedPoint(predictionDisplay[i + 1]);
            if (prA.IsBehindCamera() || prB.IsBehindCamera()) continue;
            var pA = GetWorldToScreen(prA, Camera.Current);
            var pB = GetWorldToScreen(prB, Camera.Current);

            if (predictionDisplay[i].MajorInfluence != predictionDisplay[i + 1].MajorInfluence)
            {
                //this draws the dashed blue line that shows how a body influence
                //relates to the path drawn on that body's current position.
                float totalDistance = Vector2.Distance(pA, pB);
                Vector2 direction = Vector2.Normalize(pB - pA);
                float dotLength = 10f;
                float gapLength = 10f;
                for (float dist = 0; dist < totalDistance; dist += (dotLength + gapLength))
                {
                    Vector2 startDot = pA + direction * dist;
                    Vector2 endDot = pA + direction * Math.Min(dist + dotLength, totalDistance);
                    DrawLine((int)startDot.X, (int)startDot.Y, (int)endDot.X, (int)endDot.Y, Color.SkyBlue);
                }
            }
            else
            {
                //if it's not an influence change node, draw the line
                var lineColor = predictionDisplay[i].TimeAccelerating switch
                {
                    float t when t > 8 => Color.Red,
                    float t when t > 4 => Color.Yellow,
                    float t when t > 0 => Color.Green,
                    _ => Color.Beige,
                };
                DrawLine((int)Math.Round(pA.X), (int)Math.Round(pA.Y), (int)Math.Round(pB.X), (int)Math.Round(pB.Y), lineColor);
                dtomouse = Vector2.Distance(pA, mouse_position);
                if (dtomouse < distanceToMouse)
                {
                    distanceToMouse = dtomouse;
                    closest = predictionDisplay[i];
                    closestViewPos = pA;
                }
            }
        }

        var lastPoint = shipPrediction.Points?.LastOrDefault();
        var joiningObject = lastPoint?.IsJoin ?? false;
        //draw the closest encounter points
        if (joiningObject)
        {
            var p = RelativePredictedPoint(lastPoint!);
            var viewPos = GetWorldToScreen(p, Camera.Current);
            Icons.Join.Draw(viewPos, Color.Green);
        }
        else
        {
            if (allowControl)
            {
                foreach (var encounter in shipPrediction.ClosestEncounters.Where(e => e.obj is StationaryOrbitObject))
                {
                    var inf = Game.PlayerShip.DynamicSimulation.MajorInfluenceBody;
                    if (inf != null)
                    {
                        if (inf.InHierarchy(encounter.obj)) continue;
                    }
                    var diff = encounter.time - Game.Simulation.Time;
                    var p = encounter.obj.GetPosition(encounter.time, false) + (encounter.obj.CentralBody?.GetPosition(Game.Simulation.Time) ?? Vector3D.Zero);
                    if (!p.IsBehindCamera(Camera.Current))
                    {
                        var viewPos = GetWorldToScreen(p, Camera.Current);
                        if (encounter.distance < MIN_CAPTURE_DISTANCE)
                        {
                            Icons.Join.Draw(viewPos, Color.Beige);
                            if (Vector2.Distance(mouse_position, viewPos) < 20)
                            {
                                DrawCircleLines(viewPos.X.RoundInt(), viewPos.Y.RoundInt(), 20f, Color.Beige);
                                mouse_control = false; //prevent other mouse controls
                                if (IsMouseButtonPressed(MouseButton.Left))
                                {
                                    //place a 'match velocities with target' maneuver
                                    var point = shipPrediction.Points.First(p => p.Time == encounter.time);
                                    var relVelocity = encounter.obj.GetVelocity(encounter.time) - point.Velocity;
                                    var burn = relVelocity.Magnitude() / SHIP_ACCELERATION;
                                    var burnStart = encounter.time.AddSeconds(-(burn / 2));
                                    var burnPoint = shipPrediction.Points.LastOrDefault(p => p.Time <= burnStart);
                                    if (burnPoint != null)
                                    {
                                        shipPrediction.AddManeuver(new Maneuver()
                                        {
                                            DeltaV = relVelocity,
                                            Point = burnPoint,
                                            Time = burnPoint.Time,
                                            JoinTarget = encounter.obj
                                        });
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (encounter.distance < 30)
                            {
                                //draw the ship position at encounter time
                                var point = shipPrediction.Points.FirstOrDefault(p => p.Time == encounter.time);
                                if (point != null)
                                {
                                    var sviewPos = GetWorldToScreen(RelativePredictedPoint(point), Camera.Current);
                                    DrawCircleLines(sviewPos.X.RoundInt(), sviewPos.Y.RoundInt(), 5f, Color.Green);

                                }
                                //draw object position at encounter time
                                DrawCircleLines(viewPos.X.RoundInt(), viewPos.Y.RoundInt(), 5f, Color.Green);
                            }
                        }
                    }
                }
            }
        }
        //draw a collision icon if the last predicted node is crashing into a body
        var collisionNode = shipPrediction.Points.LastOrDefault(x => x.IsCollision);
        if (collisionNode != null)
        {
            var point = RelativePredictedPoint(collisionNode);
            var viewPos = GetWorldToScreen(point, Camera.Current);
            var colText = Icons.Collision;
            DrawTexture(colText, viewPos.X.RoundInt() - colText.Width / 2, viewPos.Y.RoundInt() - colText.Height / 2, Color.White);
        }
        //draw the maneuver nodes position.
        foreach (var m in shipPrediction.Maneuvers.Where(m => m.Time >= Game.Simulation.Time))
        {
            var point = shipPrediction.Points.Last(p => p.Time == m.Time);
            var relPosition = RelativePredictedPoint(point);
            if (relPosition.IsBehindCamera()) continue;
            var viewPos = GetWorldToScreen(relPosition, Camera.Current);
            DrawCircleLines((int)Math.Round(viewPos.X), (int)Math.Round(viewPos.Y), 6f, Color.Beige);
        }
        //draw the maneuver placement gizmo
        if (mouse_control && distanceToMouse < 50)
        {
            DrawCircle((int)Math.Round(closestViewPos.X), (int)Math.Round(closestViewPos.Y), 6f, Color.Beige);
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                shipPrediction.AddManeuver(closest.Time);
            }
        }
    }
    private static Vector3D RelativePredictedPoint(PredictedPoint point)
        => point.MajorInfluence != null
            //this is so the point is shown relative to where the major body of influence is now.
            ? point.Position - point.MajorInfluence.GetPosition(point.Time) + point.MajorInfluence.GetPosition(Game.Simulation.Time)
            : point.Position;
    private static unsafe void ManeuverControls()
    {
        var shipPrediction = Game.PlayerShip.Prediction;
        var maneuver = shipPrediction.Maneuvers.Where(m => m.Time >= Game.Simulation.Time).LastOrDefault();
        if (maneuver == null) return;
        if (IsKeyPressed(KeyboardKey.Backspace))
        {
            shipPrediction.RemoveManeuver();
            return;
        }
        var maneuverPoint = shipPrediction.Points.Last(p => p.Time == maneuver.Time);
        var forwardVector = maneuverPoint.Velocity.Normalize();
        var upVector = new Vector3D(0, 1, 0);
        var planeOfReference = maneuverPoint.MajorInfluence;
        if (planeOfReference != null)
        {
            forwardVector = (maneuverPoint.Velocity - planeOfReference.GetVelocity(maneuverPoint.Time)).Normalize();
        }
        var qVector = Vector3D.Cross(forwardVector, upVector).Normalize();
        float ACCEL = MANEUVER_ACCELERATION;
        if (IsKeyDown(KeyboardKey.LeftShift)) ACCEL /= 20;
        if (IsKeyDown(KeyboardKey.W)) shipPrediction
            .AddDeltaV(forwardVector * ACCEL);
        if (IsKeyDown(KeyboardKey.S)) shipPrediction
            .AddDeltaV(forwardVector * -ACCEL);
        if (IsKeyDown(KeyboardKey.D)) shipPrediction
            .AddDeltaV(qVector * ACCEL);
        if (IsKeyDown(KeyboardKey.A)) shipPrediction
            .AddDeltaV(qVector * -ACCEL);
        if (IsKeyDown(KeyboardKey.Q)) shipPrediction
            .AddDeltaV(upVector * ACCEL);
        if (IsKeyDown(KeyboardKey.Z)) shipPrediction
            .AddDeltaV(upVector * -ACCEL);
    }
}
