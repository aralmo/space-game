global using Raylib_cs;
global using System.Numerics;
global using static Raylib_cs.Raylib;
internal class Program
{
    private static unsafe void Main(string[] args)
    {
        InitWindow(1000, 1000, "sim");
        
        SetTargetFPS(60);
        DateTime lastFrame = DateTime.UtcNow;        
        var backgroundStars = GenerateStarPositions(1000, 10000f);

        var simulation = new Simulation();        
        var sun = CelestialBody
            .Create(Vector3.Zero, 3000000f)
            .WithModelVisuals(model: null, size: 60f, Color.Yellow);

        var planet = CelestialBody
            .Create(centralBody: sun, radius: 1000f, mass: 90f, eccentricity: 0.05f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.EarthLike, 1), size: 4f, Color.Blue);

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

        var orbitingCamera = new OrbitingCamera(target: planet, initialDistance: 100.0f, initialAngle: 0.0f);

        while (!WindowShouldClose())
        {
            var delta_time = (float)(DateTime.UtcNow - lastFrame).TotalSeconds;
            simulation.SimulationTime = DateTime.UtcNow;
            lastFrame = DateTime.UtcNow;
            orbitingCamera.Update(simulation.SimulationTime);
            var camera = orbitingCamera.GetCamera();
            BeginDrawing();
            DrawBackground(camera, backgroundStars, simulation);
            simulation.DrawFarAwayBodies(camera);
            simulation.DrawOrbits2D(camera, orbitingCamera.Target);
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
            var pos = GetWorldToScreen(star+camera.Position, camera);
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
