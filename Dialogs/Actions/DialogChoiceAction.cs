public class DialogChoiceAction : DialogAction
{
    bool finished;
    public override bool Finished => finished;
    Character character;
    private readonly string characterId;
    private readonly DialogChoice[] choices;
    public int SelectedOption { get; private set; } = 0;
    public DialogChoiceAction(string characterId, params DialogChoice[] choices) : base(DialogSide.left, characterId, string.Empty)
    {
        character = Characters.Get(characterId);
        Text = new string('\n',choices.Length);
        this.characterId = characterId;
        this.choices = choices;
    }
    protected override void DrawDialogText()
    {
        if (!finished && IsKeyPressed(KeyboardKey.Down))
        {
            SelectedOption = (SelectedOption + 1) % choices.Length;
        }
        if (!finished && IsKeyPressed(KeyboardKey.Up))
        {
            SelectedOption = (SelectedOption - 1 + choices.Length) % choices.Length;
        }
        if (IsKeyPressed(KeyboardKey.Enter))
        {
            if (choices[SelectedOption].Set != null)
            {
                Game.SetFlag(choices[SelectedOption].Set!);
            }
            finished = true;
        }
        string visibleText = Text.Substring(0, CharIndex);
        string[] lines = visibleText.Split('\n');
        float lineHeight = 35f; // Adjust the line height as needed
        float textStartY = TextBubble.Y + 45; // Start drawing text a bit below the top of the text bubble
        // Draw the character name in a different color
        Color nameColor = Color.Blue; // Choose a color for the name
        DrawText(character.Name, (int)TextBubble.X + 10, (int)textStartY - 30, FONT_SIZE, nameColor);
        // Calculate the width of the longest option
        int maxWidth = 0;
        foreach (var choice in choices)
        {
            int optionWidth = MeasureText(choice.Text, FONT_SIZE);
            if (optionWidth > maxWidth)
            {
                maxWidth = optionWidth;
            }
        }

        for (int i = 0; i < choices.Length; i++)
        {
            Color optionColor = Color.Black;
            int optionWidth = MeasureText(choices[i].Text, FONT_SIZE);
            int xOffset = (int)(TextBubble.X + (TextBubble.Width - optionWidth) / 2);

            if (i == SelectedOption)
            {
                // Draw a background rectangle for the selected option
                Rectangle optionBackground = new Rectangle(xOffset - 5, textStartY + i * lineHeight - 5, optionWidth + 10, lineHeight);
                DrawRectangleRec(optionBackground, Color.Gray);
                optionColor = Color.White; // Change text color for better contrast
            }
            DrawText(choices[i].Text, xOffset, (int)(textStartY + i * lineHeight), FONT_SIZE, optionColor);
        }
    }
}
public class DialogChoice
{
    public string Text { get; }
    public string? Set { get; }
    public DialogChoice(string text, string set)

    {
        this.Text = text;
        this.Set = set;
    }
}