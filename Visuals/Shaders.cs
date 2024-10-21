public static class Shaders
{
    public static Shader Ship;
    public static void Load()
    {
        // Ship = LoadShader("shaders/ship.vs","shaders/ship.fs");
        Ship = LoadShader("shaders/diffuse.vs","shaders/diffuse.fs");
    }
    public static void Unload()
    {
        UnloadShader(Ship);
    }
}
