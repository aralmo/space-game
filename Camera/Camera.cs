public static class Camera
{
    public static Camera3D Current => current.Camera;
    static ICameraController current;
    public static void Orbit(PlayerShip ship)
    {
        current = new OrbitingCamera().SetTarget(ship.DynamicSimulation);
    }
    public static void CinematicView(PlayerShip ship)
    {
        current = new CinematicViewCamera(ship.DynamicSimulation);
    }
    public static void Update()
    {
        current?.Update();
    }
}
interface ICameraController
{
    Camera3D Camera { get; }
    void Update();
}