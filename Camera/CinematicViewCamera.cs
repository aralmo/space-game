
using System.Runtime.CompilerServices;

public class CinematicViewCamera : ICameraController
{
    public Camera3D Camera => camera;
    Camera3D camera;
    private readonly DynamicSimulation simulation;

    public CinematicViewCamera(DynamicSimulation ship)
    {
        float distance = 7f;
        Vector3 pos;
        if (ship.MajorInfluenceBody != null)
        {
            pos = ship.Position + ((ship.Position - ship.MajorInfluenceBody.GetPosition(ship.simulation.SimulationTime)).Normalize() * distance);
        }else
        {
            pos = new Vector3(0.0f, 2.0f, -distance);
        }
        this.camera = new Camera3D()
        {
            Position = pos,
            Target = ship.Position,
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 60.0f,
            Projection = CameraProjection.Perspective
        };
        this.simulation = ship;
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}