public class Object3D
{
    public Vector3 Position { get; set; }
    public float Scale { get; set; } = 1.0f;
    public float Rotation { get; set; }
    public Vector3 RotationAxis { get; set; } = new Vector3(0, 1, 0);
    public Model Model;
    public Object3D? Parent { get; set; }

    public Object3D(Vector3 position = default, Vector3 rotationAxis = default, float rotationAngle = 0, float scale = 1.0f, Model? model = null, Object3D? parent = null)
    {
        Position = position == default ? new Vector3(0, 0, 0) : position;
        RotationAxis = rotationAxis == default ? new Vector3(0, 1, 0) : rotationAxis;
        Rotation = rotationAngle;
        Scale = scale;
        Model = model ?? LoadModelFromMesh(GenMeshCube(1,1,1));
        Parent = parent;
    }

    public void Draw3D()
    {
        var modelMatrix = GetWorldMatrix();
        var p = Position;
        if (Parent != null)
        {
            var m = Matrix4x4.CreateFromAxisAngle(Parent.RotationAxis, -Parent.Rotation);
            p = Vector3.Transform(p, m) * Parent.Scale;
        }
        Model.Transform = modelMatrix;
        DrawModel(Model, p, 1.0f, Color.White);
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
}