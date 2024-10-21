public class DialogView : GameView
{
    public override bool Running => DialogController.Running;
    public override void Draw2D()
    {
        DialogController.Draw2D();
    }
}
