public class OrbitingCamera : ICameraController
{
    private Camera3D camera;
    private float cameraAngleX;
    private readonly Simulation? simulation;
    private float cameraAngleY;
    private float cameraDistance;
    bool isDynamicTarget = false;
    OrbitingObject? TargetBody { get; set; }
    DynamicSimulation? TargetDynamic { get; set; }
    public OrbitingCamera(OrbitingObject? target = null, (float x, float y) initialAngle = default, Simulation? simulation = null)
    {
        var distance = DistanceFor(target);
        this.TargetBody = target;
        camera = new Camera3D
        {
            Position = new Vector3(0.0f, 2.0f, -distance),
            Target = target?.GetPosition(DateTime.UtcNow) ?? Vector3.Zero,
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 60.0f,
            Projection = CameraProjection.Perspective
        };
        cameraDistance = distance;
        if (initialAngle != default)
        {
            cameraAngleX = initialAngle.x;
            cameraAngleY = initialAngle.y;
        }
        this.simulation = simulation;
        cameraAngleY = 0.0f;
    }

    private float DistanceFor(OrbitingObject? target)
    {
        if (target == null) return 0f;
        if (target is CelestialBody body)
        {
            return body.Size * 6f;
        }
        else
        {
            return 6f;
        }
    }

    public Camera3D Camera { get => camera; }

    public void SetTarget(OrbitingObject target)
    {
        this.TargetBody = target;
        isDynamicTarget = false;
    }
    public OrbitingCamera SetTarget(DynamicSimulation target)
    {
        this.TargetDynamic = target;
        isDynamicTarget = true;
        return this;
    }
    public void Update()
    {
        // Update camera rotation
        if (IsMouseButtonDown(MouseButton.Right))
        {
            cameraAngleX -= GetMouseDelta().X * (60f / Constants.TARGET_FPS) * .003f;
            cameraAngleY += GetMouseDelta().Y * (60f / Constants.TARGET_FPS) * .003f;
            cameraAngleY = Math.Clamp(cameraAngleY, -MathF.PI / 2, MathF.PI / 2); // Limit vertical rotation
        }

        // Update camera zoom
        cameraDistance -= GetMouseWheelMove() * 2.0f;
        cameraDistance = MathF.Max(cameraDistance, .5f); // Prevent zooming too close

        // Update camera position based on angle and distance
        var targetPosition = isDynamicTarget ? TargetDynamic!.Position : TargetBody!.GetPosition(simulation?.Time ?? DateTime.UtcNow);
        camera.Position.X = (float)(targetPosition.X + Math.Sin(cameraAngleX) * Math.Cos(cameraAngleY) * cameraDistance);
        camera.Position.Y = (float)(targetPosition.Y + Math.Sin(cameraAngleY) * cameraDistance);
        camera.Position.Z = (float)(targetPosition.Z + Math.Cos(cameraAngleX) * Math.Cos(cameraAngleY) * cameraDistance);

        camera.Target = targetPosition;
    }
}
