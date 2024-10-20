public static class ShipVisuals
{

    public static unsafe void Run()
    {
        InitWindow(2000, 1000, "sim");
        SetTargetFPS(60);
        Shaders.Load();
        var ship = new Object3D(scale: .2f)
        {
            RotationAxis = new Vector3(.5f,1f,0f)
        };
        // ship.Size = .2f;
        ship.Model = ShipModels.Load("ship1");

        var attachment = new Object3D()
        {
            Parent = ship,
            Position = new Vector3(.5f,0,0),
            Model = LoadModelFromMesh(GenMeshCube(1.0f, 1.0f, 1.0f)),
            Scale = .1f
        };

        Camera.FreeOrbit();
        while (!WindowShouldClose())
        {
            ship.Rotation += 0.001f;
            Camera.Update();
            BeginDrawing();
            ClearBackground(Color.Black);
            BeginMode3D(Camera.Current);
            Draw3DGrid(gridSize: 10, .05f, color: new Color(120, 120, 120, 120));
            ship.Draw3D();
            attachment.Draw3D();
            EndMode3D();
            DialogController.Draw2D();
            EndDrawing();
        }

        Shaders.Unload();
        ShipModels.Unload();
        CloseWindow();
    }
}