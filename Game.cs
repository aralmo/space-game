public static class Game
{

    public static unsafe void Run()
    {
        InitWindow(1000, 1000, "sim");

        SetTargetFPS(60);
        DateTime lastFrame = DateTime.UtcNow;
        var backgroundStars = GenerateStarPositions(1000, 10000f);

        var simulation = Test.DefaultSimulation();
        var targetIndex = 0;
        var target = simulation.CelestialBodies.ElementAt(targetIndex);
        var orbitingCamera = new OrbitingCamera(target, initialDistance: (float)target.Size * 6f, initialAngle: 0.0f);

        while (!WindowShouldClose())
        {
            var delta_time = (float)(DateTime.UtcNow - lastFrame).TotalSeconds;
            simulation.SimulationTime = DateTime.UtcNow;
            lastFrame = DateTime.UtcNow;

            // Check for tab key press to cycle through targets
            if (IsKeyPressed(KeyboardKey.Tab))
            {
                targetIndex = (targetIndex + 1) % simulation.CelestialBodies.Count();
                target = simulation.CelestialBodies.ElementAt(targetIndex);
                //todo: set target
                orbitingCamera = new OrbitingCamera(target, initialDistance: (float)target.Size * 6f, initialAngle: 0.0f);
            }

            orbitingCamera.Update(simulation.SimulationTime);
            var camera = orbitingCamera.GetCamera();
            BeginDrawing();
            DrawBackground(camera, backgroundStars, simulation);
            simulation.DrawFarAwayBodies(camera);
            simulation.DrawOrbits2D(camera, target);
            BeginMode3D(camera);
            simulation.Draw(camera);
            EndMode3D();
            //DrawEdit(ref orb);
            EndDrawing();
        }

        CloseWindow();
    }

    private static unsafe void DrawBackground(Camera3D camera, List<Vector3> backgroundStars, Simulation simulation)
    {
        ClearBackground(Color.Black);
        var t = (simulation.SimulationTime - DateTime.UnixEpoch).TotalSeconds * .2f % (Math.PI * 2);
        for (int i = 0; i < backgroundStars.Count; i++)
        {
            Vector3 star = backgroundStars[i];
            var pos = GetWorldToScreen(star + camera.Position, camera);
            if (pos.X < 0 || pos.X > GetScreenWidth() || pos.Y < 0 || pos.Y > GetScreenHeight())
            {
                continue;
            }
            var s = 1f;
            float blinkFactor = MathF.Sin((float)t + i) * 1f + 1f;
            s *= blinkFactor;
            DrawCircle((int)pos.X, (int)pos.Y, MathF.Max(1f, s), Color.White);
        }
    }

    static List<Vector3> GenerateStarPositions(int numberOfStars, float radius)
    {
        Random random = new Random();
        List<Vector3> stars = new List<Vector3>();

        for (int i = 0; i < numberOfStars; i++)
        {
            float t = (float)(random.NextDouble() * 2 * Math.PI);
            float p = (float)(Math.Acos(2 * random.NextDouble() - 1));
            float x = radius * MathF.Sin(p) * MathF.Cos(t);
            float y = radius * MathF.Sin(p) * MathF.Sin(t);
            float z = radius * MathF.Cos(p);
            stars.Add(new Vector3(x, y, z));
        }

        return stars;
    }
}
