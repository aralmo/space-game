
public static class Camera
{
    public static Camera3D Current => current.Camera;
    static ICameraController current;
    public static void Orbit(Transform ship)
    {
        current = new OrbitingCamera().SetTarget(ship);
    }
    public static void CinematicView(Transform ship)
    {
        //Todo: this
        //current = new CinematicViewCamera(ship);
    }
    public static void Update()
    {
        current?.Update();
    }

    internal static void FreeOrbit(Vector3? lookAt = null)
    {
        current = new FreeOrbitingCamera(lookAt ?? Vector3D.Zero);
    }
}
interface ICameraController
{
    Camera3D Camera { get; }
    void Update();
}