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
        //DialogController.Play("test");
        UInt64 iter = 0;
        while (!WindowShouldClose())
        {
            if (iter++ % TARGET_FPS == 0)
            {
                Game.CurrentMission?.Update();
            }
            Game.Simulation.Update();
            Camera.Update();
            BeginDrawing();

            background.Draw2D(Camera.Current, DateTime.UtcNow);
            Game.Simulation.Draw2D(Camera.Current);
            if (!DialogController.Running)
            {
                Maneuver();
            }
            BeginMode3D(Camera.Current);
            Game.PlayerShip.Draw3D();
            Game.Simulation.Draw3D(Camera.Current);
            EndMode3D();
            Game.CurrentMission?.Draw2D();
            DialogController.Draw2D();
            EndDrawing();
        }

        Shaders.Unload();
        ShipModels.Unload();
        //unload simualtion models when it's separated.
        CloseWindow();
    }

    static Stack<Maneuver> maneuvers = new();
    private static void Maneuver()
    {
        var points = Game.PlayerShip.DynamicSimulation.PathPredictor.Prediction;
        if (points.HasValue)
        {
            int decimation = 300;
            var dpoints = points.Value.Positions.Decimate(decimation).ToArray();
            var dtimes = points.Value.Times.Decimate(decimation).ToArray();
            var dvelocities = points.Value.Velocities.Decimate(decimation).ToArray();
            var mibp = Game.PlayerShip.DynamicSimulation.MajorInfluenceBody?.GetPosition(Game.Simulation.SimulationTime);
            float minDistance = float.MaxValue;
            Vector3D closest = Vector3D.Zero;
            Vector2 closestScreen = Vector2.Zero;
            Vector3D mv = Vector3D.Zero;            
            var mouse = GetMousePosition();
            DateTime mtime = default;
            for (int i = 0; i < dpoints.Length - 1; i++)
            {
                var worldPosA = dpoints[i] + mibp;
                var worldPosB = dpoints[i + 1] + mibp;
                var viewPosA = GetWorldToScreen(worldPosA.Value, Camera.Current);
                var viewPosB = GetWorldToScreen(worldPosB.Value, Camera.Current);
                if (worldPosA.Value.IsBehindCamera(Camera.Current) || worldPosB.Value.IsBehindCamera(Camera.Current))
                {
                    continue;
                }
                var distance = Vector2.Distance(viewPosA, mouse);
                if (distance < MIN_DISTANCE_TO_MOUSE && distance < minDistance)
                {
                    minDistance = distance;
                    closest = worldPosA.Value;
                    closestScreen = viewPosA;
                    mv = dvelocities[i];
                    mtime = dtimes[i];
                }
                DrawLine((int)Math.Round(viewPosA.X), (int)Math.Round(viewPosA.Y), (int)Math.Round(viewPosB.X), (int)Math.Round(viewPosB.Y), Color.Beige);
            }
            if (closestScreen != Vector2.Zero)
            {
                DrawCircle((int)Math.Round(closestScreen.X), (int)Math.Round(closestScreen.Y), 4f, Color.Gold);
            }
            if (IsMouseButtonPressed(MouseButton.Left) && closestScreen != Vector2.Zero)
            {
                if (!maneuvers.Any() || mtime > maneuvers.Peek().Time)
                {
                    maneuvers.Push(new Maneuver()
                    {
                        PredictedPosition = closest,
                        Time = mtime,
                        DeltaV = Vector3D.Zero,
                        StartVelocity = mv
                    });
                }
            }
        }
        else
        {
            Game.PlayerShip.DynamicSimulation.PathPredictor.Invalidate();
        }

        if (Game.PlayerShip.DynamicSimulation.MajorInfluenceBody != null)
        {
            Game.Simulation.DrawOrbits2D(Camera.Current, out Vector3D? _, Game.PlayerShip.DynamicSimulation.MajorInfluenceBody);
        }
        if (maneuvers.Any())
        {

            foreach (var man in maneuvers)
            {
                var spos = GetWorldToScreen(man.PredictedPosition, Camera.Current);
                DrawCircleLines((int)Math.Round(spos.X), (int)Math.Round(spos.Y), 4f, Color.Gold);
            }
            var maneuver = maneuvers.Peek();
            var spaceshipSimulation = Game.PlayerShip.DynamicSimulation;
            var planeOfReference = Game.PlayerShip.DynamicSimulation.MajorInfluenceBody;
            var simulation = Game.Simulation;
            var predictor = Game.PlayerShip.DynamicSimulation.PathPredictor;
            if (IsKeyPressed(KeyboardKey.Backspace))
            {
                maneuvers.Pop();
                predictor.Maneuvers = maneuvers.ToArray();
                predictor.Invalidate();
                return; //ignore this frame
            }

            var forwardVector = maneuver.StartVelocity.Normalize();
            var upVector = new Vector3D(0, 1, 0);
            if (planeOfReference != null)
            {
                forwardVector = (maneuver.StartVelocity - planeOfReference.GetVelocity(simulation.SimulationTime)).Normalize();
            }
            var qVector = Vector3D.Cross(forwardVector, upVector).Normalize();
            if (IsKeyDown(KeyboardKey.W))
            {
                maneuver.DeltaV += forwardVector * SHIP_ACCELERATION;
                predictor.Maneuvers = maneuvers.ToArray();
                predictor.Invalidate();
            }
            if (IsKeyDown(KeyboardKey.S))
            {
                predictor.Maneuvers = maneuvers.ToArray();
                predictor.Invalidate();
                maneuver.DeltaV -= forwardVector * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.D))
            {
                predictor.Maneuvers = maneuvers.ToArray();
                predictor.Invalidate();
                maneuver.DeltaV += qVector * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.A))
            {
                predictor.Maneuvers = maneuvers.ToArray();
                predictor.Invalidate();
                maneuver.DeltaV -= qVector * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.Q))
            {
                predictor.Maneuvers = maneuvers.ToArray();
                predictor.Invalidate();
                maneuver.DeltaV += upVector * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.Z))
            {
                predictor.Maneuvers = maneuvers.ToArray();
                predictor.Invalidate();
                maneuver.DeltaV -= upVector * SHIP_ACCELERATION;
            }
        }
    }
    static void SetupGame()
    {
        var simulation = Test.DefaultSimulation();
        var startVectors = ShipStartingVectors(simulation);
        var ds = new DynamicSimulation(simulation, startVectors.pos, startVectors.vel);
        ds.PathPredictor.Start();
        simulation.RegisterDynamicForUpdate(ds);
        Game.Simulation = simulation;
        Game.PlayerShip = new PlayerShip(simulation, ds, "ship1");
        Game.Simulation.ForceStep();
    }
    static (Vector3D pos, Vector3D vel) ShipStartingVectors(Simulation sim)
    {
        var planet = sim.OrbitingBodies.Skip(1).First();
        var orbit = OrbitingObject.Create(planet, 20f, 1f,sim.SimulationTime);
        return (orbit.GetPosition(sim.SimulationTime), orbit.GetVelocity(sim.SimulationTime));
    }
}
public class Maneuver
{
    public DateTime Time;
    public Vector3D DeltaV;
    public Vector3D PredictedPosition;
    public Vector3D StartVelocity;
}