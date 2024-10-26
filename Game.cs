using MissionTypes;

/// <summary>
/// Game state context
/// </summary> <summary>
/// 
/// </summary>
public static class Game
{
    public static IMission? CurrentMission { get; set; }
    public static Simulation Simulation { get; set; }
    public static List<Spaceship> Spaceships{get;} = new List<Spaceship>();
    public static Spaceship SelectedShip => Spaceships.FirstOrDefault();
    static HashSet<string> flags = new();
    public static void SetFlag(string flag)
    {
        flags.Add(flag);
    }
    public static bool HasFlag(string flag) => flags.Contains(flag);
}