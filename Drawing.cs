using static Raylib_cs.Raylib;

public static class Drawing
{
    public static void DrawLineOfPoints(IEnumerable<Vector3> vectors, Color? color = null)
    {
        var v = vectors.ToArray();
        for (int i = 0; i < v.Length; i++)
        {
            DrawLine3D(v[i], v[i < v.Length - 1 ? i + 1 : 0], (color != null) ? color.Value : Color.LightGray);
        }
    }

    public static void Draw3DGrid(int gridSize = 10, float gridSpacing = .5f, Vector3 position = default)
    {
        var gridColor = Color.Gray;
        for (int i = -gridSize; i <= gridSize; i++)
        {
            DrawLine3D(new Vector3(i * gridSpacing, 0, -gridSize * gridSpacing), new Vector3(i * gridSpacing, 0, gridSize * gridSpacing), gridColor);
            DrawLine3D(new Vector3(-gridSize * gridSpacing, 0, i * gridSpacing), new Vector3(gridSize * gridSpacing, 0, i * gridSpacing), gridColor);
        }
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

    static void WriteParameters(OrbitParameters p)
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