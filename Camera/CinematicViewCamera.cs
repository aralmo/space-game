
using System.Runtime.CompilerServices;

public class CinematicViewCamera : ICameraController
{
    Action update;
    public Camera3D Camera => camera;
    Camera3D camera;
    float t = 0f;
    public CinematicViewCamera(DynamicSimulation ship)
    {

        float distance =2f;
        this.camera = new Camera3D()
        {
            Position = GetPosition(ship, distance, 0f),
            Target = ship.Position,
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 60.0f,
            Projection = CameraProjection.Perspective
        };
        update = () =>
        {
            t += 0.0002f;
            Vector3 pos = GetPosition(ship, distance, t);

            this.camera.Position = pos;
            this.camera.Target = ship.Position;
        };
        update();
    }

    private static Vector3 GetPosition(DynamicSimulation ship, float distance, float t)
    {
        var v = (1f-MathF.Cos(t * MathF.PI)) *.5f;
        var v2 = (1f-MathF.Sin(t * MathF.PI))*.5f;

        Vector3 pos;
        if (ship.MajorInfluenceBody != null)
        {
            pos = ship.Position
            + ((ship.Position - ship.MajorInfluenceBody.GetPosition(ship.simulation.SimulationTime)).Normalize() * distance)
            + (ship.UpVector() * (1f-(.3f*v))) + (ship.Velocity.Normalize() * (1f+(.7f*v2)));
        }
        else
        {
            pos = ship.Position + new Vector3(0.0f, 2.0f, -distance);
        }

        return pos;
    }

    public void Update() => update();
} 