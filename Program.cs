global using Raylib_cs;
global using System.Numerics;
using static Raylib_cs.Raylib;
using static Drawing;
using static Model;
using System.Runtime.CompilerServices;
internal class Program
{
    private static unsafe void Main(string[] args)
    {
        float PLANET_MASS = 1000f;
        var ship_v = new Vector3(0f, 0f, Solve.OrbitVelocity(2f, PLANET_MASS));
        var ship_p = new Vector3(2f, 0f, 0f);
        // See https://aka.ms/new-console-template for more information

        InitWindow(1000, 1000, "sim");
        // Define the camera to look into our 3d world (position, target, up vector)
        Camera3D camera = new Camera3D();
        camera.Position = new Vector3(0.0f, 2.0f, -5.0f);    // Camera position
        camera.Target = new Vector3(0f, 0f, 0f);      // Camera looking at point
        camera.Up = new Vector3(0.0f, 1.0f, 0.0f);          // Camera up vector (rotation towards target)
        camera.FovY = 60.0f;                                // Camera field-of-view Y
        camera.Projection = CameraProjection.Perspective;             // Camera projection type
        DateTime lastFrame = DateTime.UtcNow;
        var orbit = Solve.KeplarOrbit(ship_p, ship_v, PLANET_MASS);
        var points = Solve.OrbitPoints(orbit, 100).ToArray();


        var icos = LoadModelFromMesh(GenMeshCustom(IcosphereGenerator.GenerateIcosphere(2).ToArray()));
        var texture = LoadTextureFromImage(GenImageCellular(512, 512, 100));
        var material = LoadMaterialDefault();
        SetMaterialTexture(ref material, MaterialMapIndex.Diffuse, texture);
        icos.Materials[0] = material;
        while (!WindowShouldClose())
        {
            var delta_time = (float)(lastFrame - DateTime.UtcNow).TotalSeconds;
            lastFrame = DateTime.UtcNow;
            (ship_p, ship_v) = Solve.ApplyGravity(ship_p, ship_v, PLANET_MASS, delta_time);

            BeginDrawing();
            ClearBackground(Color.Black);
            BeginMode3D(camera);
            //DrawSphere(new Vector3(0f, 0f, 0f), 1, Color.Blue);
            DrawModel(icos, Vector3.Zero, 1f, Color.White);
            DrawCube(ship_p, .1f, .1f, .1f, Color.Magenta);
            DrawLineOfPoints(points);
            EndMode3D();
            EndDrawing();
        }

        CloseWindow();
    }
}