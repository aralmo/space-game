
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
        var ship = OrbitingObject.Create(planet, 30, 1);
        var targetIndex = 1;
        var cameraTarget = simulation.OrbitingBodies.ElementAt(targetIndex);
        CelestialBody? target = null;
        var orbitingCamera = new OrbitingCamera(cameraTarget, initialAngle: 0.0f);
        var spaceshipSimulation = new DynamicSimulation(ship.GetPosition(lastFrame), ship.GetVelocity(lastFrame));
        string showtext = string.Empty;
        Vector3D[] predictedOrbit = Array.Empty<Vector3D>();
        OrbitingObject bodyOfReference = planet;
        while (!WindowShouldClose())
        {
            var newtime = DateTime.UtcNow;
            var delta_time = (float)(newtime - lastFrame).TotalSeconds;
            simulation.SimulationTime = newtime;
            lastFrame = newtime;

            if (IsMouseButtonPressed(MouseButton.Left))
            {
                int mouseX = GetMouseX();
                int mouseY = GetMouseY();
                target = ClickedBody(orbitingCamera.GetCamera(), simulation, mouseX, mouseY);
            }
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
            spaceshipSimulation.UpdatePosition(delta_time);

            DrawBackground(camera, backgroundStars, simulation);
            simulation.DrawFarAwayBodies(camera);
            simulation.DrawOrbits2D(camera, cameraTarget);
            BeginMode3D(camera);
            //DrawLineOfPoints(opoints.Select(p => (Vector3)p + planet.GetPosition(simulation.SimulationTime)));
            simulation.Draw(camera);
            DrawCube(spaceshipSimulation.Position, .5f, .5f, .5f, Color.Blue);
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

            var spaceshipEndPos = spaceshipSimulation.Position + spaceshipSimulation.Velocity * 10f; // Scale the velocity vector for visibility

            var shipPosition = spaceshipSimulation.Position;
            // Draw X axis (left/right) in red
            var xEndPos = shipPosition + new Vector3D(10, 0, 0);
            DrawLine3D(shipPosition, xEndPos, Color.Red);

            // Draw Y axis (up/down) in green
            var yEndPos = shipPosition + new Vector3D(0, 10, 0);
            DrawLine3D(shipPosition, yEndPos, Color.Green);

            // Draw Z axis (forward/backward) in blue
            var zEndPos = shipPosition + new Vector3D(0, 0, 10);
            DrawLine3D(shipPosition, zEndPos, Color.Blue);
            EndMode3D();
            spaceshipSimulation.ApplyGravity(simulation.SimulationTime, simulation, delta_time);
            if (target != null)
            {
                var targetPosition = target.GetPosition(simulation.SimulationTime);
                var screenPos = GetWorldToScreen(targetPosition, camera);
                var distanceToCamera = Vector3.Distance(camera.Position, targetPosition);
                float boxSize = Math.Max((float)target.Size * (1000 / distanceToCamera) * 2, 1);
                DrawRectangleLines((int)(screenPos.X - boxSize / 2), (int)(screenPos.Y - boxSize / 2), (int)boxSize, (int)boxSize, Color.Yellow);
            }
            DrawText(showtext, 10, 10, 20, Color.White);
            if (IsKeyPressed(KeyboardKey.O))
            {
                predictedOrbit = spaceshipSimulation.PredictedPath(simulation, 600, 30, bodyOfReference).ToArray();
            }

            double highestInfluence = double.MinValue;
            foreach (var obj in simulation.OrbitingBodies)
            {
                if (obj is CelestialBody body)
                {
                    var bodyPosition = body.GetPosition(simulation.SimulationTime);
                    var distance = Vector3.Distance(spaceshipSimulation.Position, bodyPosition);
                    var influence = G * body.Mass / (distance * distance);
                    if (influence < MIN_INFLUENCE) continue;
                    if (influence > highestInfluence)
                    {
                        highestInfluence = influence;
                        bodyOfReference = body;
                    }

                    var screenPos = GetWorldToScreen(bodyPosition, camera);
                    DrawText($"{influence:F4}", (int)screenPos.X, (int)screenPos.Y, 20, Color.White);
                }
            }

            Draw2DLineOfPoints(camera, predictedOrbit.Select(p => p + planet.GetPosition(simulation.SimulationTime)).ToArray(), Color.Beige, false);
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
