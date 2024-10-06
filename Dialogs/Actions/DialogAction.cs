using System.Numerics;

public class DialogAction : ScriptAction
{
    public override bool Finished => CharIndex == Text.Length;
    public override bool CanBeSkipped => true;
    public DialogSide DialogSide { get; }
    public string Text { get; }

    protected readonly Character Character;
    public DialogAction(DialogSide dialogSide, string characterId, string text)
    {
        DialogSide = dialogSide;
        Character = Characters.Get(characterId);
        Text = text.BreakUpLines(FONT_SIZE, GetScreenWidth() - 280);
    }
    protected int CharIndex = 0;
    protected Rectangle TextBubble;

    public override void Draw2D()
    {
        if (CharIndex < Text.Length)
        {
            CharIndex++;
        }
        DrawBubble();
        DrawDialogText();
    }
    protected virtual void DrawBubble()
    {
        Vector2 position;
        if (DialogSide == DialogSide.left)
        {
            position = new Vector2(10, GetScreenHeight() - 200);
        }
        else
        {
            position = new Vector2(GetScreenWidth() - 200, GetScreenHeight() - 200);
        }
        // Draw the portrait
        DrawTexture(Character.Portrait, (int)position.X, (int)position.Y, Color.White);
        // Draw the text bubble with rounded corners
        if (DialogSide == DialogSide.left)
        {
            TextBubble = new Rectangle(position.X + 230, position.Y, GetScreenWidth() - 270, 180);
        }
        else
        {
            TextBubble = new Rectangle(30, position.Y, GetScreenWidth() - 270, 180);
        }
        var brdr = new Rectangle(new Vector2(TextBubble.X - 2, TextBubble.Y - 2), new Vector2(TextBubble.Width + 4, TextBubble.Height + 4));
        DrawRectangleRounded(brdr, 0.2f, 10, Color.Black);
        DrawRectangleRounded(TextBubble, 0.2f, 10, Color.LightGray);
    }
    protected virtual void DrawDialogText()
    {
        string visibleText = Text.Substring(0, CharIndex);
        string[] lines = visibleText.Split('\n');
        float lineHeight = 35f; // Adjust the line height as needed
        float textStartY = TextBubble.Y + 45; // Start drawing text a bit below the top of the text bubble
        // Draw the character name in a different color
        Color nameColor = Color.Blue; // Choose a color for the name
        DrawText(Character.Name, (int)TextBubble.X + 10, (int)textStartY - 30, FONT_SIZE, nameColor);

        for (int i = 0; i < lines.Length; i++)
        {
            DrawText(lines[i], (int)TextBubble.X + 10, (int)(textStartY + i * lineHeight), FONT_SIZE, Color.Black);
        }
    }
}
public enum DialogSide
{
    left, right
}