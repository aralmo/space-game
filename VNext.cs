
using System.Collections.Specialized;
using System.Xml.Serialization;

public static class VNext
{
    const float SHIP_ACCELERATION = .1f;
    public static unsafe void Run()
    {
        InitWindow(1000, 1000, "sim");

        SetTargetFPS(TARGET_FPS);
        DateTime lastFrame = DateTime.UtcNow;
        var backgroundStars = GenerateStarPositions(1000, 10000f);

        var simulation = Test.DefaultSimulation2();
        var planet = simulation.OrbitingBodies.Skip(1).First();
        var ship = OrbitingObject.Create(planet, 30, 1);
        var targetIndex = 1;
        var cameraTarget = simulation.OrbitingBodies.ElementAt(targetIndex);
        CelestialBody? target = null;
        var orbitingCamera = new OrbitingCamera(cameraTarget, initialAngle: 0.0f);
        var spaceshipSimulation = new DynamicSimulation(ship.GetPosition(lastFrame), ship.GetVelocity(lastFrame));
        string showtext = string.Empty;
        OrbitingObject? planeOfReference = null;
        Vector3D[] predictedOrbit = Array.Empty<Vector3D>();
        OrbitingObject bodyOfReference = planet;
        bool needsUpdate = false;
        double currentOrbitT = 1000;
        var predictionThread = new Thread(() =>
        {
            while (true)
            {
                var (o, p) = spaceshipSimulation.PredictedPath(simulation, (int)currentOrbitT + 1, TARGET_FPS);
                if (planeOfReference == null || (p != null && p != planeOfReference)) planeOfReference = p;
                predictedOrbit = o.ToArray();
                if (planeOfReference != null)
                {
                    var orbit = Solve.KeplarOrbit(
                        spaceshipSimulation.Position - planeOfReference.GetPosition(simulation.SimulationTime),
                        spaceshipSimulation.Velocity - planeOfReference.GetVelocity(simulation.SimulationTime),
                        planeOfReference.Mass
                    );
                    if (orbit.Type == OrbitType.Elliptical) currentOrbitT = orbit.T;
                }

                for (int i = 0; i < 200; i++)
                {
                    Thread.Sleep(10);
                    if (needsUpdate) break;
                }
                needsUpdate = false;
            }
        });
        predictionThread.IsBackground = true;
        predictionThread.Priority = ThreadPriority.Lowest;
        predictionThread.Start();

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
            var forwardVector = spaceshipSimulation.Velocity.Normalize();
            var upVector = new Vector3D(0, 1, 0);
            if (planeOfReference != null)
            {
                forwardVector = (spaceshipSimulation.Velocity - planeOfReference.GetVelocity(simulation.SimulationTime)).Normalize();
            }
            var qVector = Vector3D.Cross(forwardVector, upVector).Normalize();
            var forwardEndPos = spaceshipSimulation.Position + forwardVector * 10f; // Scale the vector for visibility
            var upEndPos = spaceshipSimulation.Position + upVector * 10f; // Scale the vector for visibility
            var qEndPos = spaceshipSimulation.Position + qVector * 10f; // Scale the vector for visibility

            if (IsKeyDown(KeyboardKey.W))
            {
                needsUpdate = true;
                spaceshipSimulation.Velocity += forwardVector * delta_time * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.S))
            {
                needsUpdate = true;
                spaceshipSimulation.Velocity -= forwardVector * delta_time * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.D))
            {
                needsUpdate = true;
                spaceshipSimulation.Velocity += qVector * delta_time * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.A))
            {
                needsUpdate = true;
                spaceshipSimulation.Velocity -= qVector * delta_time * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.Q))
            {
                needsUpdate = true;
                spaceshipSimulation.Velocity += upVector * delta_time * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.Z))
            {
                needsUpdate = true;
                spaceshipSimulation.Velocity -= upVector * delta_time * SHIP_ACCELERATION;
            }
            if (IsKeyDown(KeyboardKey.C) && planeOfReference != null)
            {
                var rSpeed = Solve.OrbitVelocity(Vector3D.Distance(planeOfReference.GetPosition(simulation.SimulationTime), spaceshipSimulation.Position), planeOfReference.Mass);
                var mag = rSpeed - (spaceshipSimulation.Velocity - planeOfReference.GetVelocity(simulation.SimulationTime)).Magnitude();
                var fv = (forwardVector * mag).Normalize();

                spaceshipSimulation.Velocity += fv * delta_time * SHIP_ACCELERATION;
                needsUpdate = true;
            }

            DrawLine3D(spaceshipSimulation.Position, forwardEndPos, Color.Green); // Forward vector in green
            DrawLine3D(spaceshipSimulation.Position, upEndPos, Color.Blue); // Up vector in blue
            DrawLine3D(spaceshipSimulation.Position, qEndPos, Color.Purple); // Q vector in purple
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
            DrawText($"{predictedOrbit.Length}", 10, 10, 20, Color.Red);
            if (predictedOrbit.Length > 400)
            {
                predictedOrbit = reduceResolution(predictedOrbit, 400).ToArray();
            }
            if (bodyOfReference != null) Draw2DLineOfPoints(camera, predictedOrbit.Select(p => p + bodyOfReference.GetPosition(simulation.SimulationTime)).ToArray(), Color.Beige, false);
            //DrawEdit(ref orb);

            if (planeOfReference != null)
            {
                var radialDirection = (spaceshipSimulation.Position - planeOfReference.GetPosition(simulation.SimulationTime)).Normalize();
                var tangentialDirection = Vector3D.Cross(radialDirection, new Vector3D(0, 1, 0)).Normalize();
                DrawText($"{tangentialDirection}", 10, 50, 20, Color.White);
            }
            EndDrawing();
        }
        CloseWindow();
    }
    static IEnumerable<Vector3D> reduceResolution(Vector3D[] points, int maxResolution)
    {
        var step = points.Length / maxResolution;
        yield return points[0];
        for (int i = 1; i < points.Length - 1; i++)
        {
            if (i % step != 0) continue;
            yield return points[i];
        }
        yield return points[points.Length - 1];
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
