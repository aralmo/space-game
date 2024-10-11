public static class ShipVisuals
{

    public static unsafe void Run()
    {
        InitWindow(2000, 2000, "sim");
        SetTargetFPS(60);
        var background = new Background();
        Shaders.Load();
        SetupGame();
        Camera.Orbit(Game.PlayerShip);

        while (!WindowShouldClose())
        {
            Camera.Update();
            Game.PlayerShip.Update();
            BeginDrawing();
            background.Draw2D();
            BeginMode3D(Camera.Current);
            Game.PlayerShip.Draw3D();
            EndMode3D();
            DialogController.Draw2D();
            EndDrawing();
        }

        Shaders.Unload();
        ShipModels.Unload();
        CloseWindow();
    }

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