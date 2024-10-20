public static class ShipVisuals
{

    public static unsafe void Run(string shipFile)
    {
        InitWindow(1000, 1000, "sim");
        SetTargetFPS(60);
        Shaders.Load();
        var fi = new FileInfo(shipFile);
        var ship = Vessel.LoadFromFile(shipFile);
        Camera.FreeOrbit();
        while (!WindowShouldClose())
        {
            Camera.Update();
            ship.Update();
            BeginDrawing();
            ClearBackground(Color.Black);
            BeginMode3D(Camera.Current);
            Draw3DGrid(gridSize: 10, .05f, color: new Color(120, 120, 120, 120));
            ship.Draw3D();
            EndMode3D();
            DialogController.Draw2D();
            ControlAnimations(ship);
            EndDrawing();
            var f2 = new FileInfo(shipFile);
            if (f2.LastWriteTimeUtc > fi.LastWriteTimeUtc && (DateTime.UtcNow - f2.LastWriteTimeUtc).TotalSeconds > .2) {
                fi = f2;
                ship = Vessel.LoadFromFile(shipFile);
            }
        }

        // Shaders.Unload();
        // ShipModels.Unload();
        CloseWindow();
    }

    private static void ControlAnimations(Vessel ship)
    {
        throw new NotImplementedException();
    }
}