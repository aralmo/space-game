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
                    Camera.Orbit(Game.SelectedShip.Model);
                    break;
                case "cinematic":
                    Camera.CinematicView(Game.SelectedShip.Model);
                    break;
            }
        }
        finished = true;
    }
}