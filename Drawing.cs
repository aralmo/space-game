using static Raylib_cs.Raylib;

public static class Drawing
{
    public static void DrawLineOfPoints(IEnumerable<Vector3> vectors)
    {
        var v = vectors.ToArray();
        for (int i = 0; i < v.Length; i++)
        {
            DrawLine3D(v[i], v[i < v.Length - 1 ? i + 1 : 0], Color.Green);
        }
    }

    public static unsafe Model GetIcosphereModel(){
        var icos = LoadModelFromMesh(Procedural.GenMeshCustom(IcosphereGenerator.GenerateIcosphere(2).ToArray()));
        var texture = LoadTextureFromImage(GenImageCellular(512, 512, 100));
        var material = LoadMaterialDefault();
        SetMaterialTexture(ref material, MaterialMapIndex.Diffuse, texture);
        icos.Materials[0] = material;
        return icos;
    } 
}