public static class Icons
{
    static Texture2D? collision;
    public static Texture2D Collision { get 
        => (collision.HasValue ? collision : collision = Get("collision")).Value; }
    static Texture2D? join;
    public static Texture2D Join { get 
        => (join.HasValue ? join : join = Get("join")).Value; }
    static Dictionary<string, Texture2D> icons = new();
    static Texture2D Get(string icon)
    {
        if (!icons.TryGetValue(icon, out Texture2D texture))
        {
            var path = $"gamedata/icons/{icon}.png";
            var tx = LoadTexture(path);
            icons.Add(icon, tx);
            return tx;
        }
        return texture;
    }

    public static void Unload()
    {
        foreach (var (k, v) in icons)
        {
            UnloadTexture(v);
        }
    }
}