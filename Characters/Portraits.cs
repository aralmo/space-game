// Start of Selection
public static class Portraits
{
    static Dictionary<string, Texture2D> portraits = new();
    public static Texture2D Get(string portrait)
    {
        Texture2D texture;
        if (!portraits.TryGetValue(portrait, out texture))
        {
            var img = LoadImage($"gamedata/portraits/{portrait}.png");
            ImageResizeNN(ref img, 70, 70);
            ImageResizeNN(ref img, 200, 200);
            texture = LoadTextureFromImage(img);
            portraits.Add(portrait, texture);
            UnloadImage(img);
        }
        return texture;
    }
    public static void Unload()
    {
        foreach (var texture in portraits.Values)
        {
            UnloadTexture(texture);
        }
    }
}
