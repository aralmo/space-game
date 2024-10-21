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

public class DockingView : GameView
{
    public override bool Running => Game.PlayerShip.Stationed != null;
    public override void Enter()
    {
        base.Enter();
        Camera.CinematicView(Game.PlayerShip);
        Game.PlayerShip.EnginePlaying = false;
    }
    public override void Update()
    {
        base.Update();
        Game.Simulation.Speed = 0;
    }
}