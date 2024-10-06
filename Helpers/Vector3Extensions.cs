

public static class Vector3Extensions
{
    public static float Magnitude(this Vector3 v)
    {
        return MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }

    // Method to return the normalized vector
    public static Vector3 Normalize(this Vector3 v)
    {
        float magnitude = v.Magnitude();
        if (magnitude > 1e-5f) // Adding a small threshold to avoid division by zero
        {
            return v / magnitude;
        }
        return Vector3.Zero; // Return zero vector if the magnitude is almost zero
    }

    public static IEnumerable<Vector3D> Decimate(this IEnumerable<Vector3D> points, int maxResolution)
    {
        var pointsA = points.ToArray();
        var step = pointsA.Length / maxResolution;
        if (pointsA.Length <= maxResolution)
        {
            foreach (var p in points) yield return p;
        }
        yield return pointsA[0];
        for (int i = 1; i < pointsA.Length - 1; i++)
        {
            if (step == 0 || i % step != 0) continue;
            yield return pointsA[i];
        }
        yield return pointsA[pointsA.Length - 1];
    }

    public static (float pitch, float yaw, float roll) ToPitchYawRoll(this Vector3D lookDirection, Vector3? up = null)
    {
        if (up == null) up = new Vector3(0, 1, 0);
        var forward = (Vector3)lookDirection.Normalize();
        var right = Vector3.Cross(up.Value, forward).Normalize();
        var correctedUp = Vector3.Cross(forward, right);

        float pitch = MathF.Asin(-forward.Y);
        float yaw = MathF.Atan2(forward.X, forward.Z);
        float roll = MathF.Atan2(right.Y, correctedUp.Y);

        return (pitch, yaw, roll);
    }

    public static bool IsBehindCamera(this Vector3D point, Camera3D camera)
    => Vector3.Dot(Vector3.Normalize(point - camera.Position), camera.Target - camera.Position) <= 0;

}