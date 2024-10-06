public static class DialogController
{
    static int index = 0;
    static DialogScript script;
    public static void Play(string dialog)
    {
        index = 0;
        script = new DialogScript(dialog);
    }
    public static void Draw2D()
    {
        if (script == null || index > script.ScriptActions.Length - 1) return;
        var action = script.ScriptActions[index];
        if (action is DialogChoiceAction && action.Finished) { index++; return;}

        while (action.If != null && !action.If.All(Game.HasFlag) && index < script.ScriptActions.Length)
        {
            index++;
            if (index > script.ScriptActions.Length - 1) return;
            action = script.ScriptActions[index];
        };
        if (action.Finished && (IsKeyDown(KeyboardKey.Enter) || IsKeyPressed(KeyboardKey.RightControl)))
        {
            index++;
        }
        else if (!action.Finished && action.CanBeSkipped && (IsKeyDown(KeyboardKey.Enter) || IsKeyPressed(KeyboardKey.RightControl)))
        {
            action.Expedite();
        }
        action.Draw2D();
    }
}