global using Raylib_cs;
global using System.Numerics;
using static Raylib_cs.Raylib;
using static Drawing;
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

        SetTargetFPS(60);
        DateTime lastFrame = DateTime.UtcNow;

        float cameraDistance = 10.0f;
        float cameraAngle = 0.0f;

        var planet = CelestialBody.Create(Vector3.Zero, 9000f);
        var orb = Solve.CircularOrbit(20f, planet.Mass, DateTime.UtcNow);
        orb.Eccentricity = 0.3f;
        orb.SemiMajorAxis = 20f * 1.3f;


        while (!WindowShouldClose())
        {
            var moon = CelestialBody.Create(planet, orb, 100f);
            var points = Solve.OrbitPoints(orb, 100);
            var delta_time = (float)(lastFrame - DateTime.UtcNow).TotalSeconds;
            lastFrame = DateTime.UtcNow;
            UpdateCamera(ref camera, ref cameraAngle, ref cameraDistance, delta_time);
            BeginDrawing();
            ClearBackground(Color.Black);
            BeginMode3D(camera);
            var mpos = moon.GetPosition(lastFrame);
            DrawSphere(Vector3.Zero, 4f, Color.Blue);
            DrawSphere(mpos, 1f, Color.White);
            DrawLineOfPoints(points);

            // Draw a grid
            int gridSize = 100;
            float gridSpacing = 5.0f;
            Color gridColor = Color.Gray;

            for (int i = -gridSize; i <= gridSize; i++)
            {
                DrawLine3D(new Vector3(i * gridSpacing, 0, -gridSize * gridSpacing), new Vector3(i * gridSpacing, 0, gridSize * gridSpacing), gridColor);
                DrawLine3D(new Vector3(-gridSize * gridSpacing, 0, i * gridSpacing), new Vector3(gridSize * gridSpacing, 0, i * gridSpacing), gridColor);
            }

            EndMode3D();
            DrawEdit(ref orb);
            EndDrawing();
        }

        CloseWindow();
    }

    static (int x, int y) selected = default;
    static string input = string.Empty;
    static void DrawEdit(ref OrbitParameters pars)
    {
        int y = 10;
        int x = 10;
        int spacing = 30;
        int labelWidth = 150;
        int inputWidth = 200;


        void DrawLabel(string text, int x, int y)
        {
            DrawText(text, x, y, 20, Color.White);
        }

        void DrawFloatInput(ref float value, int x, int y)
        {
            string input = value.ToString("F2");
            input = DrawTextBox(input, x, y, inputWidth, 20);
            if (float.TryParse(input, out float result))
            {
                value = result;
            }
        }
        string DrawTextBox(string text, int x, int y, int width, int height)
        {
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mousePosition = GetMousePosition();
                if (mousePosition.X >= x && mousePosition.X <= x + width &&
                    mousePosition.Y >= y && mousePosition.Y <= y + height)
                {
                    selected = (x, y);
                    input = string.Empty;
                }
            }

            DrawRectangle(x, y, width, height, Color.DarkGray);
            if (x == selected.x && y == selected.y)
            {
                if (x == selected.x && y == selected.y)
                {
                    int key = GetKeyPressed();
                    if (key >= 32 && key <= 126) // Printable ASCII range
                    {
                        input += (char)key;
                    }
                    else if (key == 259 && input.Length > 0) // Backspace key
                    {
                        input = input.Substring(0, input.Length - 1);
                    }
                }
                DrawText(input, x + 5, y + 5, 20, Color.White);
                return input;
            }
            else
            {
                DrawText(text, x + 5, y + 5, 20, Color.White);
                return text;
            }
        }

        void DrawVector3Input(ref Vector3 value, int x, int y)
        {
            DrawLabel("X:", x, y);
            DrawFloatInput(ref value.X, x + 30, y);
            DrawLabel("Y:", x + 100, y);
            DrawFloatInput(ref value.Y, x + 130, y);
            DrawLabel("Z:", x + 200, y);
            DrawFloatInput(ref value.Z, x + 230, y);
        }

        DrawLabel("Semi-Major Axis:", x, y);
        DrawFloatInput(ref pars.SemiMajorAxis, x + labelWidth, y);
        y += spacing;

        DrawLabel("Eccentricity:", x, y);
        DrawFloatInput(ref pars.Eccentricity, x + labelWidth, y);
        y += spacing;

        DrawLabel("Inclination:", x, y);
        DrawFloatInput(ref pars.Inclination, x + labelWidth, y);
        y += spacing;

        DrawLabel("Longitude of Ascending Node:", x, y);
        DrawFloatInput(ref pars.LongitudeOfAscendingNode, x + labelWidth, y);
        y += spacing;

        DrawLabel("Argument of Periapsis:", x, y);
        DrawFloatInput(ref pars.ArgumentOfPeriapsis, x + labelWidth, y);
        y += spacing;

        DrawLabel("True Anomaly:", x, y);
        DrawFloatInput(ref pars.TrueAnomaly, x + labelWidth, y);
        y += spacing;

        DrawLabel("Mean Anomaly:", x, y);
        DrawFloatInput(ref pars.MeanAnomaly, x + labelWidth, y);
        y += spacing;

        DrawLabel("Time of Periapsis Passage:", x, y);
        DrawFloatInput(ref pars.TimeOfPeriapsisPassage, x + labelWidth, y);
        y += spacing;

        DrawLabel("Gravitational Parameter:", x, y);
        DrawFloatInput(ref pars.GravitationalParameter, x + labelWidth, y);
        y += spacing;

        DrawLabel("Asymptote Direction:", x, y);
        DrawVector3Input(ref pars.AsymptoteDirection, x + labelWidth, y);
        y += spacing;
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

    static unsafe void WriteParameters(OrbitParameters p)
    {
        var y = 5;
        foreach (var par in p.Parameters())
        {
            DrawText(par.par, 10, y, 20, Color.White);
            DrawText(par.value.ToString(), 100, y, 20, Color.White);
            y += 30;
        }
    }
}