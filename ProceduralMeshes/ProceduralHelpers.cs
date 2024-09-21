public static class Procedural
{
    public unsafe static Mesh GenMeshCustom(IEnumerable<Face> faces)
    {
        Mesh mesh = new Mesh();
        mesh.TriangleCount = faces.Count();
        mesh.VertexCount = mesh.TriangleCount * 3;

        fixed (float* vertices = new float[mesh.VertexCount * 3])
        fixed (float* normals = new float[mesh.VertexCount * 3])
        fixed (float* texCoords = new float[mesh.VertexCount * 2])
        {
            mesh.Vertices = vertices; // 3 vertices, 3 coordinates each (x, y, z)
            mesh.TexCoords = texCoords; // 3 vertices, 2 coordinates each (x, y)
            mesh.Normals = normals;   // 3 vertices, 3 coordinates each (x, y, z)

            int idx = 0;
            int tidx = 0; // texture coordinate index
            foreach (var face in faces)
            {
                for (int j = 0; j < 3; j++)
                {
                    mesh.Vertices[idx + 0] = face.Vertices[j].X;
                    mesh.Vertices[idx + 1] = face.Vertices[j].Y;
                    mesh.Vertices[idx + 2] = face.Vertices[j].Z;

                    mesh.Normals[idx + 0] = face.Normals[j].X;
                    mesh.Normals[idx + 1] = face.Normals[j].Y;
                    mesh.Normals[idx + 2] = face.Normals[j].Z;

                    mesh.TexCoords[tidx + 0] = face.UV[j].X;
                    mesh.TexCoords[tidx + 1] = face.UV[j].Y;

                    idx += 3;
                    tidx += 2;
                }
            }

            Raylib.UploadMesh(&mesh, false);
            return mesh;
        }
    }
    public static (float u, float v) MapToUV(Vector3 point)
    {
        // Normalize the point to ensure it's on the unit sphere
        point = point.Normalized();

        // Spherical coordinates to UV
        float u = .5f;//MathF.Atan2(point.Z, point.X) / (2.0f * MathF.PI);
        float v = u;

        return (u, v);
    }

    public static (Vector3 v0, Vector3 v1, Vector3 v2)[] CreateIcosahedron()
    {
        float t = (1f + MathF.Sqrt(5f)) / 2f;

        var vertices = new[] {
            new Vector3(-1f,  t,  0f),
            new Vector3( 1f,  t,  0f),
            new Vector3(-1f, -t,  0f),
            new Vector3( 1f, -t,  0f),

            new Vector3( 0f, -1f,  t),
            new Vector3( 0f,  1f,  t),
            new Vector3( 0f, -1f, -t),
            new Vector3( 0f,  1f, -t),

            new Vector3( t,  0f, -1f),
            new Vector3( t,  0f,  1f),
            new Vector3(-t,  0f, -1f),
            new Vector3(-t,  0f,  1f)
        };

        var faces = new[] {
            (vertices[0], vertices[11], vertices[5]),
            (vertices[0], vertices[5], vertices[1]),
            (vertices[0], vertices[1], vertices[7]),
            (vertices[0], vertices[7], vertices[10]),
            (vertices[0], vertices[10], vertices[11]),

            (vertices[1], vertices[5], vertices[9]),
            (vertices[5], vertices[11], vertices[4]),
            (vertices[11], vertices[10], vertices[2]),
            (vertices[10], vertices[7], vertices[6]),
            (vertices[7], vertices[1], vertices[8]),

            (vertices[3], vertices[9], vertices[4]),
            (vertices[3], vertices[4], vertices[2]),
            (vertices[3], vertices[2], vertices[6]),
            (vertices[3], vertices[6], vertices[8]),
            (vertices[3], vertices[8], vertices[9]),

            (vertices[4], vertices[9], vertices[5]),
            (vertices[2], vertices[4], vertices[11]),
            (vertices[6], vertices[2], vertices[10]),
            (vertices[8], vertices[6], vertices[7]),
            (vertices[9], vertices[8], vertices[1])
        };

        return faces;
    }
}