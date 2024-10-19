using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

public static class TestGamePhase
{

    public static unsafe void Run()
    {
        InitWindow(1000, 1000, "sim");
        SetTargetFPS(60);
        var background = new Background();
        Shaders.Load();
        SetupGame();
        Camera.Orbit(Game.PlayerShip);
        var shipPrediction = new PathPrediction(Game.Simulation, Game.PlayerShip.DynamicSimulation);
        Game.PlayerShip.DynamicSimulation.MajorInfluenceBody = shipPrediction.Points.First().MajorInfluence;
        DialogController.Play("test");
        ulong iter = 0;
        bool playing = false;
        while (!WindowShouldClose())
        {
            if (iter++ % TARGET_FPS == 0)
            {
                Game.CurrentMission?.Update();
            }
            Camera.Update();
            Game.Simulation.Update();
            Game.PlayerShip.Update();
            shipPrediction.Update();
            BeginDrawing();
            background.Draw2D(Camera.Current, DateTime.UtcNow);
            if (!DialogController.Running) { Game.Simulation.DrawOrbits2D(Camera.Current, out Vector3D? _); }
            Game.Simulation.Draw2D(Camera.Current);
            BeginMode3D(Camera.Current);
            Game.PlayerShip.Draw3D();
            Game.Simulation.Draw3D(Camera.Current);
            EndMode3D();
            Game.CurrentMission?.Draw2D();
            DialogController.Draw2D();
            if (!DialogController.Running)
            {
                DrawPredictedManeuver(shipPrediction);
                ManeuverControls(shipPrediction);
                if (IsKeyPressed(KeyboardKey.Space))
                {
                    playing = !playing;
                }
                Game.Simulation.Speed = 0;
                if (playing)
                {
                    UpdateShipPosition(shipPrediction);
                }
            }
            else
            {
                playing = false;
                Game.Simulation.Speed = 0;
            }
            //TESTDrawBodyInfluences();
            EndDrawing();
        }
        Icons.Unload();
        Shaders.Unload();
        ShipModels.Unload();
        //unload simualtion models when it's separated.
        CloseWindow();
    }

    private static unsafe void UpdateShipPosition(PathPrediction shipPrediction)
    {
        PredictedPoint? currentShipPoint = shipPrediction.Points.FirstOrDefault(t => t.Time >= Game.Simulation.Time);
        if (currentShipPoint != null)
        {
            Game.PlayerShip.DynamicSimulation.Position = currentShipPoint.Position;
            Game.PlayerShip.DynamicSimulation.Velocity = currentShipPoint.Velocity;
            Game.PlayerShip.DynamicSimulation.MajorInfluenceBody = currentShipPoint.MajorInfluence;
            Game.PlayerShip.enginePlaying = currentShipPoint.Accelerating;
            Game.Simulation.Speed = 1;
        }
        else
        {
            shipPrediction.Reset();
        }
    }

    private static unsafe void DrawPredictedManeuver(PathPrediction shipPrediction)
    {
        var predictionDisplay = shipPrediction.Points.Where(p => p.Time >= Game.Simulation.Time).Decimate(400).ToArray();
        var mpos = GetMousePosition();
        float distanceToMouse = float.MaxValue;
        PredictedPoint closest = default;
        Vector2 closestViewPos = default;
        float dtomouse;
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
                //this draws the dashed blue line that shows how a line drawn for a body influence
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
                DrawLine((int)Math.Round(pA.X), (int)Math.Round(pA.Y), (int)Math.Round(pB.X), (int)Math.Round(pB.Y), predictionDisplay[i].Accelerating ? Color.Gold : Color.Beige);
                dtomouse = Vector2.Distance(pA, mpos);
                if (dtomouse < distanceToMouse)
                {
                    distanceToMouse = dtomouse;
                    closest = predictionDisplay[i];
                    closestViewPos = pA;
                }
            }
        }
        //draw the closest encounter points
        foreach (var encounter in shipPrediction.ClosestEncounters)
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
                //todo: improve what to show
                if (encounter.distance < MIN_CAPTURE_DISTANCE)
                {
                    Icons.Join.Draw(viewPos, Color.Green);
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
        if (distanceToMouse < 50)
        {
            DrawCircle((int)Math.Round(closestViewPos.X), (int)Math.Round(closestViewPos.Y), 6f, Color.Beige);
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                shipPrediction.AddManeuver(closest.Time);
            }
        }
    }

    private static unsafe void ManeuverControls(PathPrediction shipPrediction)
    {
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

    private static unsafe void TESTDrawBodyInfluences()
    {
        foreach (var body in Game.Simulation.OrbitingBodies.Where(b => b is CelestialBody))
        {
            var bpos = body.GetPosition(Game.Simulation.Time);
            if (bpos.IsBehindCamera(Camera.Current)) continue;
            var viewPos = GetWorldToScreen(bpos, Camera.Current);
            var influence = Solve.Influence(Game.PlayerShip.DynamicSimulation.Position, bpos, body.Mass);
            DrawText($"{influence}", (int)viewPos.X, (int)viewPos.Y, 25, Color.Orange);
        }
    }

    private static Vector3D RelativePredictedPoint(PredictedPoint point)
        => point.MajorInfluence != null
            //this is so the point is shown relative to where the major body of influence is now.
            ? point.Position - point.MajorInfluence.GetPosition(point.Time) + point.MajorInfluence.GetPosition(Game.Simulation.Time)
            : point.Position;
    static void SetupGame()
    {
        var simulation = Test.DefaultSimulation();
        var startVectors = ShipStartingVectors(simulation);
        var ds = new DynamicSimulation(simulation, startVectors.pos, startVectors.vel);
        Game.Simulation = simulation;
        Game.PlayerShip = new PlayerShip(simulation, ds, "ship1");
    }
    static (Vector3D pos, Vector3D vel) ShipStartingVectors(Simulation sim)
    {
        var planet = sim.OrbitingBodies.Skip(1).First();
        var orbit = OrbitingObject.Create(planet, 20f, 1f, sim.Time);
        return (orbit.GetPosition(sim.Time), orbit.GetVelocity(sim.Time));
    }
}