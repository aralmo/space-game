
public struct Vector3D
{
    public double X;
    public double Y;
    public double Z;
    public static Vector3D Zero => new Vector3D(0, 0, 0);

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3D operator -(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3D operator *(Vector3D a, double scalar)
    {
        return new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static Vector3D operator /(Vector3D a, double scalar)
    {
        return new Vector3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
    }
    public double Magnitude()
    {
        return Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public double Length() => Magnitude();

    public Vector3D Normalize()
    {
        double length = Length();
        return new Vector3D(X / length, Y / length, Z / length);
    }

    public static double Dot(Vector3D a, Vector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public static Vector3D Cross(Vector3D a, Vector3D b)
    {
        return new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }
    public static Vector3D operator *(double scalar, Vector3D a)
    {
        return new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static Vector3D operator /(double scalar, Vector3D a)
    {
        return new Vector3D(scalar / a.X, scalar / a.Y, scalar / a.Z);
    }

    public static Vector3D operator +(double scalar, Vector3D a)
    {
        return new Vector3D(a.X + scalar, a.Y + scalar, a.Z + scalar);
    }

    public static Vector3D operator -(double scalar, Vector3D a)
    {
        return new Vector3D(scalar - a.X, scalar - a.Y, scalar - a.Z);
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    internal static double Distance(Vector3 position1, Vector3D position2)
    {
        double deltaX = position1.X - position2.X;
        double deltaY = position1.Y - position2.Y;
        double deltaZ = position1.Z - position2.Z;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }

    public static implicit operator Vector3(Vector3D v)
    {
        return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
    }

}
