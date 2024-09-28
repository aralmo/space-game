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
}