using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public static class VNext
{
    public static unsafe void Run()
    {
        InitWindow(1000, 1000, "sim");
        SetTargetFPS(TARGET_FPS);
        var background = new Background();
        Shaders.Load();
        SetupGame();
        Camera.Orbit(Game.PlayerShip);
        ulong iter = 0;
        GameView[] Views = [
            new DialogView(),
            new DockingView(),
            new PlayTurnView(),
            new PlanningView(),
        ];
        GameView? lastView = null;
        while (!WindowShouldClose())
        {
            //view selector
            var view = Views.First(v => v.Running);
            if (lastView == null || view != lastView)
            {
                view.Enter();
                lastView?.Exit();
            }
            lastView = view;
            //1 per second updates
            if (iter++ % TARGET_FPS == 0)
            {
                Game.CurrentMission?.Update();
            }

            //full time updates
            Camera.Update();
            Game.Simulation.Update();
            Game.PlayerShip.Update();
            view.Update();

            //pre-3d 2d drawing
            BeginDrawing();
            background.Draw2D(Camera.Current, DateTime.UtcNow);
            Game.Simulation.Draw2D(Camera.Current);
            view.Draw2D();

            //3d drawing
            BeginMode3D(Camera.Current);
            Game.PlayerShip.Draw3D();
            Game.Simulation.Draw3D(Camera.Current);
            view.Draw3D();

            //post-3d 2d drawing
            EndMode3D();
            Game.CurrentMission?.Draw2D();
            view.Draw2DAfter();
            EndDrawing();
        }
        UnloadResources();
        CloseWindow();
    }

    private static unsafe void UnloadResources()
    {
        Icons.Unload();
        Shaders.Unload();
        ShipModels.Unload();
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