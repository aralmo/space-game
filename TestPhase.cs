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
            if (!DialogController.Running)
            {
                DrawManeuverGUI();
            }
            BeginDrawing();
            background.Draw2D(Camera.Current, DateTime.UtcNow);
            Game.Simulation.Draw2D(Camera.Current);

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

    private static void DrawManeuverGUI()
    {
        Game.PlayerShip.DynamicSimulation.PathPredictor.DrawPredictedPath2D(Camera.Current);
        if (Game.PlayerShip.DynamicSimulation.MajorInfluenceBody != null)
        {
            Game.Simulation.DrawOrbits2D(Camera.Current, Game.PlayerShip.DynamicSimulation.MajorInfluenceBody);
        }
    }

    static void DrawGameUI2D()
    {

    }

    static void SetupGame()
    {
        var simulation = Test.DefaultSimulation2();
        var startVectors = ShipStartingVectors(simulation);
        var ds = new DynamicSimulation(simulation, startVectors.pos, startVectors.vel);
        ds.PathPredictor.Start();
        simulation.RegisterDynamicForUpdate(ds);
        Game.Simulation = simulation;
        Game.PlayerShip = new PlayerShip(simulation, ds, "ship1");
        //
        Game.Simulation.ForceStep();
    }
    static (Vector3D pos, Vector3D vel) ShipStartingVectors(Simulation sim)
    {
        var planet = sim.OrbitingBodies.Skip(1).First();
        var orbit = OrbitingObject.Create(planet, 20f, 1f);
        return (orbit.GetPosition(sim.SimulationTime), orbit.GetVelocity(sim.SimulationTime));
    }
}
