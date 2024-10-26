
public static class Experiments
{

    public static unsafe void Run()
    {
        string shipFile = $"gamedata/ships/pioneer.json";

        InitWindow(1000, 1000, "sim");
        SetTargetFPS(60);
        Shaders.Load();
        var ship = ShipModel.LoadFromFile(shipFile);
        var t = (ship.Transforms.First(t => t is ModelTransform) as ModelTransform)!;
        var mesh = t.Model.Meshes[0];
        Camera.FreeOrbit();
        var panel = GetPanel();
        while (!WindowShouldClose())
        {
            Camera.Update();
            ship.Update();
            BeginDrawing();
            ClearBackground(Color.Black);
            BeginMode3D(Camera.Current);
            var ray = GetMouseRay(GetMousePosition(), Camera.Current);
            if (IsMouseButtonDown(MouseButton.Left))
            {
                var c = GetRayCollisionMesh(ray, mesh, t.Model.Transform);
                if (c.Hit)
                {
                    DarkenVertice(ref mesh, c.Point, t.Model.Transform);
                    Console.WriteLine("hit");
                }
            }
            Draw3DGrid(gridSize: 10, .05f, color: new Color(120, 120, 120, 120));
            ship.Draw3D();
            EndMode3D();
            panel.Draw(0,0, 200, GetScreenHeight());
            DialogController.Draw2D();
            EndDrawing();
        }

        Shaders.Unload();
        Ship3DModels.Unload();
        CloseWindow();
    }

    private static unsafe void DarkenVertice(ref Mesh mesh, Vector3 point, Matrix4x4 transform)
    {
        fixed (byte* color = new byte[4])
        {
            color[3] = 255;
            for (int i = 0; i < mesh.VertexCount*3 ; i += 3)
            {
                var x = mesh.Vertices[i];
                var y = mesh.Vertices[i + 1];
                var z = mesh.Vertices[i + 2];
                var disttocurrent = Vector3.Distance(Vector3.Transform(new Vector3(x, y, z), transform), point);
                if (disttocurrent < .003f)
                {
                    UpdateMeshBuffer(mesh, 3,color, 4, i/3*4);
                }
            }
        }
        // fixed (float* data = new float[3])
        // {
        //     UpdateMeshBuffer(mesh, 0, data,3, closer);
        // }

        // fixed (byte* color = new byte[12])
        // {
        //     color[3] = 255;
        //     color[7] = 255;
        //     color[11] = 255;
        //     UpdateMeshBuffer(mesh, 3, color, 12, closer * 4);
        // }
    }

    static UIPanel GetPanel()
    {
        return new UIPanel(320,1f){
            AlignRight = true,
            Children = [
                new UILabel(1f,40,"delta v"),
                new UIBar(1f, 40, Color.Green, ()=> .75f),
                new UILabel(1f,40,"some buttons"),
                new UIButton(40, "some", () => Console.Write("click 1")),
                new UIButton(40, "2", () => Console.Write("click 2")),
                new UIButton(40, "2", () => Console.Write("click 2")),
                new UIButton(50, 40, "2", () => Console.Write("click 2")),
                new UIButton(50, 40, "2", () => Console.Write("click 2")),
                new UIButton(50, 40, "2", () => Console.Write("click 2")),
                new UIButton(50, 40, "2", () => Console.Write("click 2")),
                new UIButton(50, 40, "2", () => Console.Write("click 2")),
            ]
        };
    }
}