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
    public static PlayerShip PlayerShip { get; set; }
    static HashSet<string> flags = new();
    public static void SetFlag(string flag)
    {
        flags.Add(flag);
    }
    public static bool HasFlag(string flag) => flags.Contains(flag);
}