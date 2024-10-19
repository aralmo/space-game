using MissionTypes;

public static class Missions
{
    static Dictionary<string, IMission> missions;
    static Missions()
    {
        missions = new(){
            {"tutorial", new Tutorial()}
        };
    }
    public static void Start(string mission)
    {
        Game.CurrentMission = missions[mission];
    }
}