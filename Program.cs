global using Raylib_cs;
global using System.Numerics;
global using static Raylib_cs.Raylib;
global using static Drawing;
internal class Program
{
    private static unsafe void Main(string[] args)
    {
        InitWindow(1000, 1000, "sim");
        Camera3D camera = new Camera3D();
        camera.Position = new Vector3(0.0f, 2.0f, -10.0f);
        camera.Target = new Vector3(0f, 0f, 0f);
        camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
        camera.FovY = 60.0f;
        camera.Projection = CameraProjection.Perspective;
        //Rlgl.SetClipPlanes(0.1f,10000f);
        SetTargetFPS(60);
        DateTime lastFrame = DateTime.UtcNow;
        float cameraDistance = 10.0f;
        float cameraAngle = 0.0f;
        var backgroundStars = GenerateStarPositions(1000, 10000f);
        var simulation = new Simulation();
        
        var sun = CelestialBody
            .Create(Vector3.Zero, 30000f)
            .WithModelVisuals(model: null, size: 10f);

        var planet = CelestialBody
            .Create(centralBody: sun, radius: 200f, mass: 90f, eccentricity: 0.05f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.EarthLike, 1), size: 2f);

        simulation
            .AddCelestialBody(sun)
            .AddCelestialBody(planet)
            .AddCelestialBody(CelestialBody
                .Create(centralBody: planet, radius: 41f, mass: 90f, eccentricity: 0.12f, inclination: 1.67f, argumentOfPeriapsis: 1f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 2), size: 1f))
            .AddCelestialBody(CelestialBody
                .Create(centralBody: planet, radius: 76f, mass: 65f, eccentricity: 0.23f, inclination: 1.73f, argumentOfPeriapsis: 2.3f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 3), size: 0.7f))
            .AddCelestialBody(CelestialBody
                .Create(centralBody: planet, radius: 126f, mass: 44f, eccentricity: -0.17f, inclination: 0.23f, argumentOfPeriapsis: 4.2f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.IcePlanet, 4), size: 0.9f));

        while (!WindowShouldClose())
        {
            var delta_time = (float)(DateTime.UtcNow - lastFrame).TotalSeconds;
            simulation.SimulationTime = DateTime.UtcNow;
            lastFrame = DateTime.UtcNow;
            UpdateCamera(ref camera, ref cameraAngle, ref cameraDistance, delta_time);
            BeginDrawing();
            DrawBackground(camera, backgroundStars, simulation);
            simulation.DrawOrbits2D(camera);
            BeginMode3D(camera);
            simulation.Draw();
            //Draw3DGrid(200, 5f);
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
            var pos = GetWorldToScreen(star, camera);
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

    static void UpdateCamera(ref Camera3D camera, ref float cameraAngle, ref float cameraDistance, float delta_time)
    {
        // Update camera rotation
        if (IsKeyDown(KeyboardKey.A))
        {
            cameraAngle -= 1.0f * delta_time;
        }
        if (IsKeyDown(KeyboardKey.D))
        {
            cameraAngle += 1.0f * delta_time;
        }

        // Update camera zoom
        cameraDistance -= GetMouseWheelMove() * 2.0f;
        cameraDistance = MathF.Max(cameraDistance, 2.0f); // Prevent zooming too close

        // Update camera position based on angle and distance
        camera.Position.X = MathF.Sin(cameraAngle) * cameraDistance;
        camera.Position.Z = MathF.Cos(cameraAngle) * cameraDistance;

        // Adjust camera Y position based on zoom level
        if (cameraDistance > 10.0f) // Threshold for zooming out
        {
            camera.Position.Y = cameraDistance - 10.0f; // Move camera up as it zooms out
        }
        else
        {
            camera.Position.Y = 2.0f; // Default Y position
        }
    }

}
