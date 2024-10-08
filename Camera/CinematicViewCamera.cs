
using System.Runtime.CompilerServices;

public class CinematicViewCamera : ICameraController
{
    Action update;
    public Camera3D Camera => camera;
    Camera3D camera;
    float t = 0f;
    public CinematicViewCamera(DynamicSimulation ship)
    {

        float distance = 2f;
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
            t += 0.0005f;
            Vector3 pos = GetPosition(ship, distance, t);
            this.camera.Position = pos;
            this.camera.Target = ship.Position;
        };
        update();
    }

    private static Vector3 GetPosition(DynamicSimulation ship, float distance, float t)
    {
        Vector3 pos;
        if (ship.MajorInfluenceBody != null)
        {
            var rel_pos = (ship.Position - ship.MajorInfluenceBody.GetPosition(ship.simulation.SimulationTime)).Normalize() * distance;
            var up = ship.UpVector();
            var fwd = -Vector3.Cross(rel_pos, up).Normalize();
            float sinComponent = (float)Math.Sin(t) * 0.5f; // Adjust the multiplier for smoothness
            float cosComponent = (float)Math.Cos(t) * 0.5f; // Adjust the multiplier for smoothness
            up *= 1.0f + sinComponent; // Apply the sin component to the up vector
            fwd *= 1.0f + cosComponent; // Apply the cos component to the forward vector

            if (up.Y > 0)
            {
                pos = ship.Position
                + rel_pos + up + fwd;
            }else{
                pos = ship.Position
                + rel_pos - up - fwd;
            }

        }
        else
        {
            pos = ship.Position + new Vector3(0.0f, 2.0f, -distance);
        }

        return pos;
    }

    public void Update() => update();
}