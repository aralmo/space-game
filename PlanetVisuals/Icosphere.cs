public static class Icosphere
{

    public static unsafe Mesh GenerateIcospherePlanet(int subdivisions, float radius, Image heightMap, (float height, Color color)[] heightColors, float heightMultiplier = 0.1f)
    {
        Mesh mesh = new Mesh();
        mesh.VertexCount = 0;
        mesh.TriangleCount = 0;

        // Create initial icosahedron
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Color> colors = new List<Color>();

        float t = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;

        vertices.Add(new Vector3(-1, t, 0).Normalize() * radius);
        vertices.Add(new Vector3(1, t, 0).Normalize() * radius);
        vertices.Add(new Vector3(-1, -t, 0).Normalize() * radius);
        vertices.Add(new Vector3(1, -t, 0).Normalize() * radius);

        vertices.Add(new Vector3(0, -1, t).Normalize() * radius);
        vertices.Add(new Vector3(0, 1, t).Normalize() * radius);
        vertices.Add(new Vector3(0, -1, -t).Normalize() * radius);
        vertices.Add(new Vector3(0, 1, -t).Normalize() * radius);

        vertices.Add(new Vector3(t, 0, -1).Normalize() * radius);
        vertices.Add(new Vector3(t, 0, 1).Normalize() * radius);
        vertices.Add(new Vector3(-t, 0, -1).Normalize() * radius);
        vertices.Add(new Vector3(-t, 0, 1).Normalize() * radius);

        indices.AddRange(new int[] {
        0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
        1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
        3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
        4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
    });

        // Subdivide the icosahedron
        Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

        for (int i = 0; i < subdivisions; i++)
        {
            List<int> newIndices = new List<int>();

            for (int j = 0; j < indices.Count; j += 3)
            {
                int v1 = indices[j];
                int v2 = indices[j + 1];
                int v3 = indices[j + 2];

                int a = GetMiddlePoint(v1, v2, ref vertices, ref middlePointIndexCache, radius);
                int b = GetMiddlePoint(v2, v3, ref vertices, ref middlePointIndexCache, radius);
                int c = GetMiddlePoint(v3, v1, ref vertices, ref middlePointIndexCache, radius);

                newIndices.AddRange(new int[] { v1, a, c });
                newIndices.AddRange(new int[] { v2, b, a });
                newIndices.AddRange(new int[] { v3, c, b });
                newIndices.AddRange(new int[] { a, b, c });
            }

            indices = newIndices;
        }

        // Apply height map and assign colors
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            float u = 0.5f + (MathF.Atan2(vertex.Z, vertex.X) / (2 * MathF.PI));
            float v = 0.5f - (MathF.Asin(vertex.Y) / MathF.PI);

            Color color = GetImageColor(heightMap, (int)(u * heightMap.Width), (int)(v * heightMap.Height));
            float height = color.R / 255.0f;

            vertices[i] = vertex * (1.0f + height * heightMultiplier);

            // Calculate the slope at the current vertex
            Vector3 normal = Vector3.Normalize(vertex);
            float slope = MathF.Acos(Vector3.Dot(normal, Vector3.UnitY));

            // Assign color based on height and HeightColors parameter
            if (heightColors != null)
            {
                for (int j = 0; j < heightColors.Length; j++)
                {
                    if (height <= heightColors[j].height)
                    {
                        
                        colors.Add(heightColors[j].color);
                        break;
                    }
                }
            }
        }

        mesh.VertexCount = vertices.Count;
        mesh.TriangleCount = indices.Count / 3;
        fixed (float* verticesPtr = new float[vertices.Count * 3])
        fixed (ushort* indicesPtr = indices.Select(i => (ushort)i).ToArray())
        fixed (byte* colorsPtr = new byte[colors.Count * 4])
        {
            mesh.Vertices = verticesPtr;
            mesh.Indices = indicesPtr;
            mesh.Colors = colorsPtr;

            for (int i = 0; i < vertices.Count; i++)
            {
                mesh.Vertices[i * 3] = vertices[i].X;
                mesh.Vertices[i * 3 + 1] = vertices[i].Y;
                mesh.Vertices[i * 3 + 2] = vertices[i].Z;

                mesh.Colors[i * 4] = colors[i].R;
                mesh.Colors[i * 4 + 1] = colors[i].G;
                mesh.Colors[i * 4 + 2] = colors[i].B;
                mesh.Colors[i * 4 + 3] = colors[i].A;
            }

            for (int i = 0; i < indices.Count; i++)
            {
                mesh.Indices[i] = (ushort)indices[i];
            }
            UploadMesh(ref mesh, false);
            return mesh;
        }
    }

    private static int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
    {
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        if (cache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3(
            (point1.X + point2.X) / 2.0f,
            (point1.Y + point2.Y) / 2.0f,
            (point1.Z + point2.Z) / 2.0f
        ).Normalize() * radius;

        int i = vertices.Count;
        vertices.Add(middle);

        cache.Add(key, i);

        return i;
    }
}