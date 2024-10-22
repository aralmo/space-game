public static class ShipModels
{
    static List<Model> loaded = new List<Model>();
    public unsafe static Model Load(string name)
    {
        var file = $"gamedata/ships/models/{name}.m3d";
        var pmodel = LoadModel(file);
        var model = FixVoxelModelNormals(ref pmodel);

        UnloadModel(pmodel);

        for (int i = 0; i < model.MaterialCount; i++)
        {
            model.Materials[i].Shader = Shaders.Ship;
        }
        loaded.Add(model);
        return model;
    }
    public unsafe static ModelAnimation[] LoadAnimations(string name)
    {
        var file = $"gamedata/ships/models/{name}.m3d";
        if (!File.Exists(file)) return Array.Empty<ModelAnimation>();
        int animsCount = 0;
        var anims = LoadModelAnimations(file, ref animsCount);
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
    //m3d normals get fucked up during export, vox are not imported at all by raylib
    //with this both should be ok.
    private static unsafe Model FixVoxelModelNormals(ref Model inmodel)
    {
        var model = LoadModelFromMesh(FixMeshNormals(ref inmodel.Meshes[0], inmodel.BoneCount > 0));
        fixed (Raylib_cs.Transform* transforms = new Raylib_cs.Transform[inmodel.BoneCount])
        fixed (BoneInfo* bones = new BoneInfo[inmodel.BoneCount])
        {
            model.BindPose = transforms;
            model.Bones = bones;
            model.BoneCount = inmodel.BoneCount;
            for (int i = 0; i < model.BoneCount; i++)
            {
                model.Bones[i] = inmodel.Bones[i];
                model.BindPose[i] = inmodel.BindPose[i];
            }
        }
        return model;
    }

    private unsafe static Mesh FixMeshNormals(ref Mesh inmesh, bool includeBones)
    {
        var mesh = new Mesh(inmesh.VertexCount, inmesh.TriangleCount * 3);
        fixed (float* verticesPtr = new float[inmesh.VertexCount * 3])
        fixed (float* animVerticesPtr = new float[inmesh.VertexCount * 3])
        fixed (float* normals = new float[inmesh.VertexCount * 3])
        fixed (float* animNormals = new float[inmesh.VertexCount * 3])
        fixed (float* boneWeights = new float[inmesh.VertexCount * 4])
        fixed (byte* boneIds = new byte[inmesh.VertexCount * 4])
        fixed (byte* colorsPtr = new byte[inmesh.VertexCount * 4])
        {
            mesh.Vertices = verticesPtr;
            mesh.Normals = normals;
            mesh.Colors = colorsPtr;
            mesh.BoneIds = boneIds;
            mesh.BoneWeights = boneWeights;
            mesh.AnimVertices = animVerticesPtr;
            mesh.AnimNormals = animNormals;

            var xplanen = new Vector3(1, 0, 0);
            var yplanen = new Vector3(0, 1, 0);
            var zplanen = new Vector3(0, 0, 1);
            for (int i = 0; i < inmesh.VertexCount * 3; i += 9)
            {
                mesh.Vertices[i] = inmesh.Vertices[i];
                mesh.Vertices[i + 1] = inmesh.Vertices[i + 1];
                mesh.Vertices[i + 2] = inmesh.Vertices[i + 2];
                mesh.Vertices[i + 3] = inmesh.Vertices[i + 3];
                mesh.Vertices[i + 4] = inmesh.Vertices[i + 4];
                mesh.Vertices[i + 5] = inmesh.Vertices[i + 5];
                mesh.Vertices[i + 6] = inmesh.Vertices[i + 6];
                mesh.Vertices[i + 7] = inmesh.Vertices[i + 7];
                mesh.Vertices[i + 8] = inmesh.Vertices[i + 8];

                Vector3 normal = Vector3.Zero;
                if (mesh.Vertices[i] == mesh.Vertices[i + 3] && mesh.Vertices[i] == mesh.Vertices[i + 6])
                {
                    normal = xplanen;
                }
                else
                if (mesh.Vertices[i + 1] == mesh.Vertices[i + 3 + 1] && mesh.Vertices[i + 1] == mesh.Vertices[i + 6 + 1])
                {
                    normal = yplanen;
                }
                if (mesh.Vertices[i + 1 + 1] == mesh.Vertices[i + 3 + 1 + 1] && mesh.Vertices[i + 1 + 1] == mesh.Vertices[i + 6 + 1 + 1])
                {
                    normal = zplanen;
                }

                mesh.Normals[i] = normal.X;
                mesh.Normals[i + 1] = normal.Y;
                mesh.Normals[i + 2] = normal.Z;
                mesh.Normals[i + 3] = normal.X;
                mesh.Normals[i + 4] = normal.Y;
                mesh.Normals[i + 5] = normal.Z;
                mesh.Normals[i + 6] = normal.X;
                mesh.Normals[i + 7] = normal.Y;
                mesh.Normals[i + 8] = normal.Z;
            }
            for (int i = 0; i < inmesh.VertexCount * 3; i++)
            {
                mesh.AnimVertices[i] = mesh.Vertices[i];
                mesh.AnimNormals[i] = mesh.Normals[i];
            }

            for (int i = 0; i < inmesh.VertexCount * 4; i++)
            {
                mesh.Colors[i] = inmesh.Colors[i];
                if (includeBones)
                {
                    mesh.BoneIds[i] = inmesh.BoneIds[i];
                    mesh.BoneWeights[i] = inmesh.BoneWeights[i];
                }
            }
            UploadMesh(ref mesh, true);
            return mesh;
        }
    }
}