
public class CinematicViewCamera : ICameraController
{
    Action update;
    public Camera3D Camera => camera;
    Camera3D camera;
    float t = 0f;
    public CinematicViewCamera(PlayerShip ship)
    {
        var influence = (OrbitingObject) ship.Stationed ?? ship.DynamicSimulation.MajorInfluenceBody;
        float distance = .5f;
        this.camera = new Camera3D()
        {
            Position = GetPosition(influence,ship.DynamicSimulation, distance, 0f),
            Target = ship.DynamicSimulation.Position,
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 60.0f,
            Projection = CameraProjection.Perspective
        };
        update = () =>
        {
            t += 0.0005f;
            Vector3 pos = GetPosition(influence,ship.DynamicSimulation, distance, t);
            this.camera.Position = pos;
            this.camera.Target = ship.DynamicSimulation.Position;
        };
        update();
    }

    private static Vector3 GetPosition(OrbitingObject? influence, DynamicSimulation ship, float distance, float t)
    {
        Vector3 pos;
        if (influence != null)
        {
            var rel_pos = (ship.Position - influence.GetPosition(ship.simulation.Time)).Normalize() * distance;
            var up = ship.UpVector();
            var fwd = -Vector3.Cross(rel_pos, up).Normalize();
            float sinComponent = (float)Math.Sin(t) * 0.2f; // Adjust the multiplier for smoothness
            float cosComponent = (float)Math.Cos(t) * 0.2f; // Adjust the multiplier for smoothness
            up *= sinComponent; // Apply the sin component to the up vector
            fwd *= cosComponent; // Apply the cos component to the forward vector

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