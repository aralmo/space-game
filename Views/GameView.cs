public abstract class GameView
{
    public abstract bool Running { get; }
    public virtual void Init() { }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Draw2D() { }
    public virtual void Draw2DAfter() { }
    public virtual void Draw3D() { }
    public virtual void Update() { }
}