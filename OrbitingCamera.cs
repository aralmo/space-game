public class OrbitingCamera
{
    private Camera3D camera;
    private float cameraAngle;
    private float cameraDistance;
    public OrbitingObject Target{get; private set;}
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
        cameraAngle = initialAngle;
        lastUpdateTime = DateTime.UtcNow;
    }

    private float DistanceFor(OrbitingObject target)
    {
        if (target is CelestialBody body){
            return body.Size * 6f;
        }else{
            return 6f;
        }
    }

    public Camera3D GetCamera() => camera;

    public void Update(DateTime simulationTime)
    {
        var deltaTime = (float)(simulationTime - lastUpdateTime).TotalSeconds;
        lastUpdateTime = simulationTime;

        // Update camera rotation
        if (IsKeyDown(KeyboardKey.A))
        {
            cameraAngle -= 1.0f * deltaTime;
        }
        if (IsKeyDown(KeyboardKey.D))
        {
            cameraAngle += 1.0f * deltaTime;
        }

        // Update camera zoom
        cameraDistance -= GetMouseWheelMove() * 2.0f;
        cameraDistance = MathF.Max(cameraDistance, 2.0f); // Prevent zooming too close

        // Update camera position based on angle and distance
        var targetPosition = Target.GetPosition(simulationTime);
        camera.Position.X =(float) (targetPosition.X + Math.Sin(cameraAngle) * cameraDistance);
        camera.Position.Z =(float) (targetPosition.Z + Math.Cos(cameraAngle) * cameraDistance);

        // Adjust camera Y position based on zoom level
        if (cameraDistance > 10.0f) // Threshold for zooming out
        {
            camera.Position.Y =(float) (targetPosition.Y + cameraDistance - 10.0f); // Move camera up as it zooms out
        }
        else
        {
            camera.Position.Y =(float) (targetPosition.Y + 2.0f); // Default Y position
        }

        camera.Target = targetPosition;
    }
}

