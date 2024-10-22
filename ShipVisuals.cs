public static class ShipVisuals
{

    public static unsafe void Run()
    {
        string[] ships  = ["pioneer", "warship", "shuttle"];
        var current = 0;
        string shipFile = $"gamedata/ships/{ships[current]}.json";

        InitWindow(1000, 1000, "sim");
        SetTargetFPS(60);
        Shaders.Load();
        var fi = new FileInfo(shipFile);
        var ship = Vessel.LoadFromFile(shipFile);
        Camera.FreeOrbit();
        while (!WindowShouldClose())
        {
            if (IsKeyDown(KeyboardKey.Left)) ship.Rotation -= 0.005f;
            if (IsKeyDown(KeyboardKey.Right)) ship.Rotation += 0.005f;
            Camera.Update();
            ship.Update();
            BeginDrawing();
            ClearBackground(Color.Black);
            BeginMode3D(Camera.Current);
            Draw3DGrid(gridSize: 10, .05f, color: new Color(120, 120, 120, 120));
            DrawHangarLines(ship);
            ship.Draw3D();
            EndMode3D();
            DialogController.Draw2D();
            ControlAnimations(ship);
            int screenWidth = GetScreenWidth();
            int buttonWidth = 50;
            int buttonHeight = 30;
            int posXLeftButton = screenWidth / 2 - buttonWidth - 10; // Position for "<" button
            int posY = 10; // Top margin
            int posXRightButton = screenWidth / 2 + 10; // Position for ">" button

            if (DrawUI.DrawButton(posXLeftButton, posY, buttonWidth, buttonHeight, Color.LightGray, Color.Black, "<"))
            {
                current = (current - 1 + ships.Length) % ships.Length;
                shipFile = $"gamedata/ships/{ships[current]}.json";
                ship = Vessel.LoadFromFile(shipFile);
            }

            if (DrawUI.DrawButton(posXRightButton, posY, buttonWidth, buttonHeight, Color.LightGray, Color.Black, ">"))
            {
                current = (current + 1) % ships.Length;
                shipFile = $"gamedata/ships/{ships[current]}.json";
                ship = Vessel.LoadFromFile(shipFile);
            }

            EndDrawing();
            var f2 = new FileInfo(shipFile);
            if (f2.LastWriteTimeUtc > fi.LastWriteTimeUtc && (DateTime.UtcNow - f2.LastWriteTimeUtc).TotalSeconds > .2)
            {
                fi = f2;
                ship = Vessel.LoadFromFile(shipFile);
            }
        }

        Shaders.Unload();
        ShipModels.Unload();
        CloseWindow();
    }

    private static unsafe void DrawHangarLines(Vessel ship)
    {
        if (ship.hangars == null) return;
        foreach (var hangar in ship.hangars)
        {
            DrawLineOfPoints(hangar.Route.Select(p => ship.TransformPoint(p)).Select(p => new Vector3D(p.X, p.Y, p.Z)), Color.Yellow, false);
        }
    }

    private static void ControlAnimations(Vessel ship)
    {
        DrawText("Animations", 10, 10, 20, Color.RayWhite);
        if (ship.Animations == null) return;
        int buttonX = 10; // Starting X position for the buttons
        int buttonY = 40; // Starting Y position for the buttons
        int buttonWidth = 150; // Button width
        int buttonHeight = 30; // Button height
        int buttonPadding = 10; // Padding between buttons

        foreach (var animation in ship.Animations)
        {
            if (DrawUI.DrawButton(buttonX, buttonY, buttonWidth, buttonHeight, Color.LightGray, Color.Black, animation))
            {
                ship.SwitchAnimation(animation);
            }
            buttonY += buttonHeight + buttonPadding; // Move to the next button position
        }
    }
}