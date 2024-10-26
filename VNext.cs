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
        Camera.Orbit(Game.SelectedShip.Model);
        ulong iter = 0;
        GameView[] Views = [
            new DialogView(),
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
            foreach (var ship in Game.Spaceships)
            {
                ship.Update();
            }

            view.Update();

            //pre-3d 2d drawing
            BeginDrawing();
            background.Draw2D(Camera.Current, DateTime.UtcNow);
            Game.Simulation.Draw2D(Camera.Current);
            view.Draw2D();

            //3d drawing
            BeginMode3D(Camera.Current);
            foreach (var ship in Game.Spaceships)
            {
                ship.Draw3D();
            }
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
        Ship3DModels.Unload();
    }
    static void SetupGame()
    {
        var simulation = Test.DefaultSimulation();
        Game.Simulation = simulation;
        var startVectors = ShipStartingVectors(simulation);
        var ds = new DynamicSimulation(startVectors.pos, startVectors.vel);
        Game.Spaceships.Add(new Spaceship()
        {
            Simulation = ds,
            Model = ShipModel.Load("pioneer"),
            Owner = 0,
            Prediction = new PathPrediction(Game.Simulation, ds)
        });
    }
    static (Vector3D pos, Vector3D vel) ShipStartingVectors(Simulation sim)
    {
        var planet = sim.OrbitingBodies.Skip(1).First();
        var orbit = OrbitingObject.Create(planet, 20f, 1f, sim.Time);
        return (orbit.GetPosition(sim.Time), orbit.GetVelocity(sim.Time));
    }
}