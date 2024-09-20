global using Raylib_cs;
global using System.Numerics;
using static Raylib_cs.Raylib;

var ship_v = new Vector3(0f,0f,Solve.OrbitVelocity(2f, 1000f)*1.5f);
var ship_p = new Vector3(2f,0f,0f);
var orbit = Solve.OrbitFor(ship_p,ship_v,1000f);
var points = Solve.OrbitCartesianPoints(orbit,100).ToArray();

// See https://aka.ms/new-console-template for more information

InitWindow(800, 480, "Hello World");

// Define the camera to look into our 3d world (position, target, up vector)
Camera3D camera = new Camera3D();
camera.Position = new Vector3(0.0f, 2.0f, 4.0f);    // Camera position
camera.Target = new Vector3(0f, 0f, 0f);      // Camera looking at point
camera.Up = new Vector3(0.0f, 1.0f, 0.0f);          // Camera up vector (rotation towards target)
camera.FovY = 60.0f;                                // Camera field-of-view Y
camera.Projection = CameraProjection.Perspective;             // Camera projection type

//EnableEventWaiting();
while (!WindowShouldClose())
{
    BeginDrawing();
    ClearBackground(Color.Black);
    BeginMode3D(camera);
    DrawSphere(new Vector3(0f, 0f, 0f), 1, Color.Blue);
    DrawOrbitPoints(points);
    EndMode3D();
    EndDrawing();
}

Raylib.CloseWindow();

void DrawOrbitPoints(IEnumerable<Vector3> vectors)
{
    var v = vectors.ToArray();
    for(int i = 0; i < v.Length; i++){
        DrawLine3D(v[i], v[i<v.Length-1?i+1:0], Color.Green);
    }
}
IEnumerable<Vector3> CirclePoints(int points, float distance)
{
    var diff = 2 * MathF.PI / points;
    for (int n = 0; n < points; n++)
    {
        yield return new Vector3(MathF.Cos(diff * n) * distance, 0f, MathF.Sin(diff * n) * distance);
    }
}