public class FreeOrbitingCamera : ICameraController
{
    private Camera3D camera;
    private float cameraAngleX;
    private float cameraAngleY;
    private float cameraDistance;
    private Vector3 targetPosition;

    public FreeOrbitingCamera(Vector3 target, (float x, float y) initialAngle = default, float initialDistance = 5f)
    {
        targetPosition = target;
        cameraDistance = initialDistance;
        cameraAngleX = initialAngle.x;
        cameraAngleY = initialAngle.y;

        camera = new Camera3D
        {
            Position = new Vector3(0.0f, 2.0f, -cameraDistance),
            Target = targetPosition,
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 60.0f,
            Projection = CameraProjection.Perspective
        };
    }

    public Camera3D Camera { get => camera; }

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
        camera.Position.X = (float)(targetPosition.X + Math.Sin(cameraAngleX) * Math.Cos(cameraAngleY) * cameraDistance);
        camera.Position.Y = (float)(targetPosition.Y + Math.Sin(cameraAngleY) * cameraDistance);
        camera.Position.Z = (float)(targetPosition.Z + Math.Cos(cameraAngleX) * Math.Cos(cameraAngleY) * cameraDistance);

        camera.Target = targetPosition;
    }
}
