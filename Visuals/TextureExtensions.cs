public static class TextureExtensions
{
    public static void Draw(this Texture2D texture, Vector2 position, Color tint)
    {
        DrawTexture(texture, position.X.RoundInt() - texture.Width / 2, position.Y.RoundInt() - texture.Height / 2, tint);
    }
}