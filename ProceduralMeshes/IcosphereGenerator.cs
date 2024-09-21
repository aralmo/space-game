public struct Face
{
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public Vector2[] UV;
}

public static class IcosphereGenerator
{
    private static readonly float T = (1f + (float)Math.Sqrt(5f)) / 2f;
    private static readonly float SCALE = (float)Math.Sqrt(1 + T * T);

    private static readonly Vector3[] ICOSAHEDRON_VERTICES = {
        new Vector3(-1f, T, 0f) / SCALE, new Vector3(1f, T, 0f) / SCALE,
        new Vector3(-1f, -T, 0f) / SCALE, new Vector3(1f, -T, 0f) / SCALE,
        new Vector3(0f, -1f, T) / SCALE, new Vector3(0f, 1f, T) / SCALE,
        new Vector3(0f, -1f, -T) / SCALE, new Vector3(0f, 1f, -T) / SCALE,
        new Vector3(T, 0f, -1f) / SCALE, new Vector3(T, 0f, 1f) / SCALE,
        new Vector3(-T, 0f, -1f) / SCALE, new Vector3(-T, 0f, 1f) / SCALE
    };

    private static readonly Vector3[] ICOSAHEDRON_INDICES = {
        new Vector3(0, 11, 5), new Vector3(0, 5, 1), new Vector3(0, 1, 7),
        new Vector3(0, 7, 10), new Vector3(0, 10, 11), new Vector3(1, 5, 9),
        new Vector3(5, 11, 4), new Vector3(11, 10, 2), new Vector3(10, 7, 6),
        new Vector3(7, 1, 8), new Vector3(3, 9, 4), new Vector3(3, 4, 2),
        new Vector3(3, 2, 6), new Vector3(3, 6, 8), new Vector3(3, 8, 9),
        new Vector3(4, 9, 5), new Vector3(2, 4, 11), new Vector3(6, 2, 10),
        new Vector3(8, 6, 7), new Vector3(9, 8, 1)
    };

    public static IEnumerable<Face> GenerateIcosphere(int subdivisions)
    {
        var faces = new List<Face>();

        foreach (var index in ICOSAHEDRON_INDICES)
        {
            var v0 = ICOSAHEDRON_VERTICES[(int)index.X];
            var v1 = ICOSAHEDRON_VERTICES[(int)index.Y];
            var v2 = ICOSAHEDRON_VERTICES[(int)index.Z];

            SubdivideFace(v0, v1, v2, subdivisions, faces);
        }

        return faces;
    }

    private static void SubdivideFace(Vector3 v0, Vector3 v1, Vector3 v2, int depth, List<Face> faces)
    {
        if (depth == 0)
        {
            faces.Add(CreateFace(v0, v1, v2));
        }
        else
        {
            var v01 = Vector3.Normalize((v0 + v1) / 2);
            var v12 = Vector3.Normalize((v1 + v2) / 2);
            var v20 = Vector3.Normalize((v2 + v0) / 2);

            SubdivideFace(v0, v01, v20, depth - 1, faces);
            SubdivideFace(v1, v12, v01, depth - 1, faces);
            SubdivideFace(v2, v20, v12, depth - 1, faces);
            SubdivideFace(v01, v12, v20, depth - 1, faces);
        }
    }

    private static Face CreateFace(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        // Make sure vertices follow a consistent winding order for correct face orientation
        var normal = Vector3.Cross(v1 - v0, v2 - v0);
        if (Vector3.Dot(normal, v0) < 0)
        {
            var tmp = v1;
            v1 = v2;
            v2 = tmp;
        }

        var vertices = new[] { v0, v1, v2 };
        var normals = new[] { Vector3.Normalize(v0), Vector3.Normalize(v1), Vector3.Normalize(v2) };
        var uv = new Vector2[3];
        
        for (int i = 0; i < 3; i++)
        {
            uv[i] = new Vector2(
                (float)(0.5 + (Math.Atan2(vertices[i].Z, vertices[i].X) / (2 * Math.PI))),
                (float)(0.5 - (Math.Asin(vertices[i].Y) / Math.PI))
            );
        }

        // Fix potential seam issues
        FixSeam(uv);

        return new Face
        {
            Vertices = vertices,
            Normals = normals,
            UV = uv
        };
    }

    private static void FixSeam(Vector2[] uv)
    {
        // Max UV difference before wrapping
        const float seamThreshold = 0.9f;
        
        for (int i = 0; i < uv.Length; i++)
        {
            for (int j = i + 1; j < uv.Length; j++)
            {
                if (Math.Abs(uv[i].X - uv[j].X) > seamThreshold)
                {
                    if (uv[i].X > uv[j].X)
                    {
                        uv[i].X -= 1.0f;
                    }
                    else
                    {
                        uv[j].X -= 1.0f;
                    }
                }
            }
        }
    }
}