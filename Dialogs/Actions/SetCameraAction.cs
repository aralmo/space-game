public class SetCameraAction : ScriptAction
{
    bool finished;
    public override bool Finished => finished;
    public override bool MoveForwardOnFinish => true;
    private readonly string who;
    private readonly string camera;

    public SetCameraAction(string who, string camera)
    {
        this.who = who;
        this.camera = camera;
    }
    public override void Start()
    {
        if (!finished)
        {
            switch (camera)
            {
                case "orbit":
                    Camera.Orbit(Game.PlayerShip);
                    break;
                case "cinematic":
                    Camera.CinematicView(Game.PlayerShip);
                    break;
            }
        }
        finished = true;
    }
}