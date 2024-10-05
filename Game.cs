public static class Game
{

    public static unsafe void Run()
    {
        InitWindow(1000, 1000, "sim");

        SetTargetFPS(60);
        var background = new Background();
        Shaders.Load();
        //todo: scenario editor / loader
        var simulation = Test.DefaultSimulation2();
        var startVectors = ShipStartingVectors(simulation);
        var ship = new DynamicSimulation(simulation, startVectors.pos, startVectors.vel);
        ship.PathPredictor.Start();
        simulation.RegisterDynamicForUpdate(ship);
        var model = ShipModels.Load("ship1");
        var orbiting = new OrbitingCamera();
        orbiting.SetTarget(ship);
        while (!WindowShouldClose())
        {
            simulation.Update();
            orbiting.Update(simulation.SimulationTime);
            BeginDrawing();
            background.Draw2D(orbiting.Camera, DateTime.UtcNow);
            simulation.Draw2D(orbiting.Camera);
            ship.PathPredictor.DrawPredictedPath2D(orbiting.Camera);
            if (ship.MajorInfluenceBody != null)
            {
                simulation.DrawOrbits2D(orbiting.Camera, ship.MajorInfluenceBody);
            }
            BeginMode3D(orbiting.Camera);
            ship.Draw3D(model);
            simulation.Draw3D(orbiting.Camera);
            EndMode3D();
            //DrawEdit(ref orb);
            EndDrawing();
        }

        Shaders.Unload();
        ShipModels.Unload();
        //unload simualtion models when it's separated.
        CloseWindow();
    }

    static (Vector3D pos, Vector3D vel) ShipStartingVectors(Simulation sim)
    {
        var planet = sim.OrbitingBodies.Skip(1).First();
        var orbit = OrbitingObject.Create(planet, 20f, 1f);
        return (orbit.GetPosition(sim.SimulationTime), orbit.GetVelocity(sim.SimulationTime));
    }
}
