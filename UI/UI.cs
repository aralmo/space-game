public static class DrawUI
{
    static Color BUTTON_COLOR = new Color(200, 200, 200, 255);
    static Color BUTTON_SELECTED_COLOR = new Color(100, 100, 100, 255);
    static int? lastSpeed = null;
    public static void SimSpeedControls(int x, int y)
    {
        const int buttonWidth = 50;
        const int buttonHeight = 40;
        const int spacing = 10;
        int currentX = x;
        string[] buttonLabels = { "||", ">", ">>", ">>>" };
        int[] speed = [0, 1, 3, 10];
        for (int i = 0; i < buttonLabels.Length; i++)
        {
            string? label = buttonLabels[i];
            if (DrawButton(currentX, y, buttonWidth, buttonHeight,color:
                speed[i] == Game.Simulation.Speed 
                ? BUTTON_SELECTED_COLOR : BUTTON_COLOR))
            {
                Game.Simulation.Speed = speed[i];
            }
            DrawText(label, currentX + (buttonWidth / 2) - (MeasureText(label, 20) / 2), y + (buttonHeight / 2) - 10, 20, Color.Black);
            currentX += buttonWidth + spacing;
        }

        if (IsKeyPressed(KeyboardKey.Space))
        {
            if (Game.Simulation.Speed == 0)
            {
                Game.Simulation.Speed = lastSpeed ?? 1;
            }
            else
            {
                lastSpeed = Game.Simulation.Speed;
                Game.Simulation.Speed = 0;
            }
        }
    }
    private static bool DrawButton(int x, int y, int width, int height, Color color)
    {
        // Draw shadow
        DrawRectangle(x + 1, y + 1, width, height, Color.DarkGray);
        if (IsMouseOver(x, y, width, height))
        {
            DrawRectangle(x, y, width, height, new Color((int)color.R, color.G, color.B, 100));
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                return true;
            }
        }
        else
        {
            DrawRectangle(x, y, width, height, color);
        }

        return false;
    }

    private static bool IsMouseOver(int x, int y, int width, int height)
    {
        // Assuming GetMousePosition() returns the current mouse position as a Point
        var mousePosition = GetMousePosition();
        return mousePosition.X >= x && mousePosition.X <= x + width && mousePosition.Y >= y && mousePosition.Y <= y + height;
    }

}