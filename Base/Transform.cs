public class Transform
{
    public Vector3 Position { get; set; }
    public float Scale { get; set; } = 1.0f;
    public float Rotation { get; set; }
    public Vector3 RotationAxis { get; set; } = new Vector3(0, 1, 0);
    public Transform? Parent { get; set; }

    public Transform(Vector3 position = default, Vector3 rotationAxis = default, float rotationAngle = 0, float scale = 1.0f, Transform? parent = null)
    {
        Position = position == default ? new Vector3(0, 0, 0) : position;
        RotationAxis = rotationAxis == default ? new Vector3(0, 1, 0) : rotationAxis;
        Rotation = rotationAngle;
        Scale = scale;
        Parent = parent;
    }
    public Matrix4x4 GetWorldMatrix()
    {
        var scaleMatrix = Matrix4x4.CreateScale(Scale, Scale, Scale);
        var rotationMatrix = Matrix4x4.CreateFromAxisAngle(RotationAxis, Rotation);
        var localMatrix = scaleMatrix * rotationMatrix;
        if (Parent != null)
        {
            var parentMatrix = Parent.GetWorldMatrix();
            return parentMatrix * localMatrix;
        }

        return localMatrix;
    }
    public Vector3 TransformPoint(Vector3 point)
    {
            var p = point;
            var m = Matrix4x4.CreateFromAxisAngle(RotationAxis, -Rotation);
            p = Vector3.Transform(p, m) * Scale;
            if (Parent != null)
            {
                p = Parent.TransformPoint(p);
            }       
            return p;     
    }
    protected Vector3 TransformPointByParent(Vector3 point)
    {
        var p = point;
        if (Parent != null)
        {
            var m = Matrix4x4.CreateFromAxisAngle(Parent.RotationAxis, -Parent.Rotation);
            p = Vector3.Transform(p, m) * Parent.Scale;
            p += Parent.Position;
        }
        return p;
    }
}