public static class Icons
{
    static Dictionary<string, Texture2D> icons = new();
    public static Texture2D Get(string icon)
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