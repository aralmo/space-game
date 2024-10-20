public class ModelTransform : Transform, I3DDrawable, IUpdatable
{
    public Model Model;
    public ShipAnimation[] Animations;
    public ModelTransform(Model? model = null, Vector3 position = default, Vector3 rotationAxis = default, float rotationAngle = 0, float scale = 1.0f, Transform? parent = null) : base(position, rotationAxis, rotationAngle, scale, parent)
    {
        Model = model ?? LoadModelFromMesh(GenMeshCube(1, 1, 1));
    }

    public void Draw3D()
    {
        var modelMatrix = GetWorldMatrix();
        var p = TransformPoint(Position);
        Model.Transform = modelMatrix;
        DrawModel(Model, p, 1.0f, Color.White);
    }

    public void Update()
    {
        foreach (ShipAnimation animation in Animations)
        {
            if (animation.Playing && animation.CurrentFrame < animation.Frames)
            {
                animation.CurrentFrame++;
                UpdateModelAnimation(Model, animation.ModelAnimation, animation.CurrentFrame);
            }
            else if (!animation.Playing && animation.CurrentFrame > 0)
            {
                animation.CurrentFrame--;
                UpdateModelAnimation(Model, animation.ModelAnimation, animation.CurrentFrame);
            }

        }
    }
}