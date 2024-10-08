public static class ShipModels
{
    static List<Model> loaded = new List<Model>();
    public unsafe static Model Load(string name)
    {
        var model = LoadModel($"gamedata/ships/models/{name}.m3d");
        for (int i = 0; i < model.MaterialCount; i++)
        {
            model.Materials[i].Shader = Shaders.Ship;
        }
        loaded.Add(model);
        return model;
    }
    public unsafe static ModelAnimation[] LoadAnimations(string name)
    {
        int animsCount = 0;
        var anims = LoadModelAnimations($"gamedata/ships/models/{name}.m3d", ref animsCount);
        var arr = new ModelAnimation[animsCount];
        for (int i = 0; i < animsCount; i++)
        {
            arr[i] = anims[i];
        }
        return arr;
    }

    public static void Unload()
    {
        foreach (var m in loaded)
        {
            UnloadModel(m);
        }
    }
}