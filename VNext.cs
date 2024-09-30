
using System.Collections.Specialized;
using System.Xml.Serialization;

public static class VNext
{

    public static unsafe void Run()
    {
        InitWindow(1000, 1000, "sim");

        SetTargetFPS(60);
        DateTime lastFrame = DateTime.UtcNow;
        var backgroundStars = GenerateStarPositions(1000, 10000f);

        var simulation = Test.DefaultSimulation();
        var planet = simulation.OrbitingBodies.Skip(1).First();
        var ship = OrbitingObject.Create(planet, 10, 1);
        var targetIndex = 1;
        var cameraTarget = simulation.OrbitingBodies.ElementAt(targetIndex);
        CelestialBody? target = null;
        var orbitingCamera = new OrbitingCamera(cameraTarget, initialAngle: 0.0f);
        var spaceship_vel = ship.GetVelocity(lastFrame);
        var spaceship_pos = ship.GetPosition(lastFrame);

        while (!WindowShouldClose())
        {
            var newtime = DateTime.UtcNow;
            var delta_time = (float)(newtime - lastFrame).TotalSeconds;
            simulation.SimulationTime = newtime;
            lastFrame = newtime;

            // Check for tab key press to cycle through targets
            if (IsKeyPressed(KeyboardKey.Tab))
            {
                targetIndex = (targetIndex + 1) % simulation.OrbitingBodies.Count();
                cameraTarget = simulation.OrbitingBodies.ElementAt(targetIndex);
                float distance;
                if (cameraTarget is CelestialBody body)
                {
                    distance = body.Size * 6f;
                }
                else
                {
                    distance = 6f;
                }
                orbitingCamera = new OrbitingCamera(cameraTarget, initialAngle: 0.0f);
            }
            if (IsMouseButtonDown(MouseButton.Left))
            {
                int mouseX = GetMouseX();
                int mouseY = GetMouseY();
                target = ClickedBody(orbitingCamera.GetCamera(), simulation, mouseX, mouseY);
            }

            orbitingCamera.Update(simulation.SimulationTime);
            var opoints = ship.OrbitPoints;
            var camera = orbitingCamera.GetCamera();
            BeginDrawing();
            spaceship_pos += spaceship_vel * delta_time;

            DrawBackground(camera, backgroundStars, simulation);
            simulation.DrawFarAwayBodies(camera);
            simulation.DrawOrbits2D(camera, cameraTarget);
            BeginMode3D(camera);
            DrawLineOfPoints(opoints.Select(p => (Vector3)p + planet.GetPosition(simulation.SimulationTime)));
            simulation.Draw(camera);
            DrawCube(spaceship_pos, .5f, .5f, .5f, Color.Blue);
            foreach (var body in simulation.OrbitingBodies)
            {
                var bodyPosition = body.GetPosition(simulation.SimulationTime);
                if (body.OrbitParameters != null)
                {
                    var bodyVelocity = body.OrbitParameters.Value.VelocityAtTime(simulation.SimulationTime);
                    var bodyEndPosition = bodyPosition + bodyVelocity * 10f; // Scale the velocity vector for visibility
                    DrawLine3D(bodyPosition, bodyEndPosition, Color.Red);
                }
            }

            var spaceshipEndPos = spaceship_pos + spaceship_vel * 10f; // Scale the velocity vector for visibility
            DrawLine3D(spaceship_pos, spaceshipEndPos, Color.Green);
            EndMode3D();

            foreach (var obj in simulation.OrbitingBodies)
            {
                if (obj is CelestialBody body)
                {
                    var bodyPosition = body.GetPosition(simulation.SimulationTime);
                    var screenPos = GetWorldToScreen(bodyPosition, camera);
                    var distance = Vector3.Distance(spaceship_pos, bodyPosition);
                    var influence = G * body.Mass / (distance * distance);

                    (_, spaceship_vel) = Solve.ApplyGravity(
                         position: spaceship_pos,
                         velocity: spaceship_vel,
                         planetPosition: body.GetPosition(simulation.SimulationTime),
                         planetMass: body.Mass,
                         stepTimeSeconds: delta_time);

                    DrawText($"{body.Name}: {influence:F4}", (int)Math.Round(screenPos.X), (int)Math.Round(screenPos.Y), 20, Color.White);
                }
            }
            //DrawEdit(ref orb);
            EndDrawing();
        }
        CloseWindow();
    }

    static CelestialBody? ClickedBody(Camera3D camera, Simulation simulation, int mouseX, int mouseY)
    {
        foreach (var obj in simulation.OrbitingBodies)
        {
            if (obj is CelestialBody body)
            {
                var bpos = body.GetPosition(simulation.SimulationTime);
                var screenPos = GetWorldToScreen(bpos, camera);
                var distanceToMouse = Math.Sqrt(Math.Pow(screenPos.X - mouseX, 2) + Math.Pow(screenPos.Y - mouseY, 2));
                var distanceToCamera = Vector3.Distance(camera.Position, bpos);
                double sizeFactor = 1000 / distanceToCamera;
                float drawSize = (float)Math.Max(1f, body.Size * sizeFactor);
                if (distanceToMouse <= drawSize)
                {
                    return body;
                }
            }
        }
        return null;
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
            float p = (float)Math.Acos(2 * random.NextDouble() - 1);
            float x = radius * MathF.Sin(p) * MathF.Cos(t);
            float y = radius * MathF.Sin(p) * MathF.Sin(t);
            float z = radius * MathF.Cos(p);
            stars.Add(new Vector3(x, y, z));
        }

        return stars;
    }
}
