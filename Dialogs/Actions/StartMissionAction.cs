public class StartMissionAction : ScriptAction
{
    bool finished;
    private readonly string mission;

    public override bool Finished => finished;
    public override bool MoveForwardOnFinish => true;
    public StartMissionAction(string mission)
    {
        this.mission = mission;
    }
    public override void Start()
    {
        if (!finished) Missions.Start(mission);
        finished = true;
    }
}