

public static class Vector3Extensions
{
    public static float Magnitude(this Vector3 v)
    {
        return MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }

    // Method to return the normalized vector
    public static Vector3 Normalized(this Vector3 v)
    {
        float magnitude = v.Magnitude();
        if (magnitude > 1e-5f) // Adding a small threshold to avoid division by zero
        {
            return v / magnitude;
        }
        return Vector3.Zero; // Return zero vector if the magnitude is almost zero
    }
}