public class OrbitingCamera
{
    private Camera3D camera;
    private float cameraAngleX;
    private float cameraAngleY;
    private float cameraDistance;
    public OrbitingObject Target { get; private set; }
    private DateTime lastUpdateTime;

    public OrbitingCamera(OrbitingObject target, float initialAngle)
    {
        var distance = DistanceFor(target);
        this.Target = target;
        camera = new Camera3D
        {
            Position = new Vector3(0.0f, 2.0f, -distance),
            Target = target.GetPosition(DateTime.UtcNow),
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 60.0f,
            Projection = CameraProjection.Perspective
        };
        cameraDistance = distance;
        cameraAngleX = initialAngle;
        cameraAngleY = 0.0f;
        lastUpdateTime = DateTime.UtcNow;
    }

    private float DistanceFor(OrbitingObject target)
    {
        if (target is CelestialBody body)
        {
            return body.Size * 6f;
        }
        else
        {
            return 6f;
        }
    }

    public Camera3D GetCamera() => camera;

    public void Update(DateTime simulationTime)
    {
        var deltaTime = (float)(simulationTime - lastUpdateTime).TotalSeconds;
        lastUpdateTime = simulationTime;

        // Update camera rotation
        if (IsMouseButtonDown(MouseButton.Right))
        {
            cameraAngleX -= GetMouseDelta().X * 0.005f;
            cameraAngleY += GetMouseDelta().Y * 0.005f;
            cameraAngleY = Math.Clamp(cameraAngleY, -MathF.PI / 2, MathF.PI / 2); // Limit vertical rotation
        }

        // Update camera zoom
        cameraDistance -= GetMouseWheelMove() * 2.0f;
        cameraDistance = MathF.Max(cameraDistance, 2.0f); // Prevent zooming too close

        // Update camera position based on angle and distance
        var targetPosition = Target.GetPosition(simulationTime);
        camera.Position.X = (float)(targetPosition.X + Math.Sin(cameraAngleX) * Math.Cos(cameraAngleY) * cameraDistance);
        camera.Position.Y = (float)(targetPosition.Y + Math.Sin(cameraAngleY) * cameraDistance);
        camera.Position.Z = (float)(targetPosition.Z + Math.Cos(cameraAngleX) * Math.Cos(cameraAngleY) * cameraDistance);

        camera.Target = targetPosition;
    }
}
