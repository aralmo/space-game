// Start of Selection
public abstract class ScriptAction
{
    public virtual string[]? If {get; set;}
    public virtual bool CanBeSkipped { get => false; }
    public virtual bool MoveForwardOnFinish {get => false;}
    public virtual bool Finished { get => true; }
    public virtual void Start() { }
    public virtual void Expedite() { }
    public virtual void End() { }
    public virtual void Draw2D() { }
    public virtual void Draw3D(Camera3D camera) { }
}
