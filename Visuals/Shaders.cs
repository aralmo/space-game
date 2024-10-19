public static class Shaders
{
    public static Shader Ship;
    public static void Load()
    {
        Ship = LoadShader("shaders/ship.vs","shaders/ship.fs");
    }
    public static void Unload()
    {
        UnloadShader(Ship);
    }
}
