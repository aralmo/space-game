public static class Game
{
    public static Simulation Simulation {get;set;}    
    public static PlayerShip PlayerShip {get;set;}
    static HashSet<string> flags = new();
    public static void SetFlag(string flag)
    {
        flags.Add(flag);
    }
    public static bool HasFlag(string flag) => flags.Contains(flag);
}