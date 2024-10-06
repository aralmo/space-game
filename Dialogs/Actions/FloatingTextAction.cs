using System.Security.Cryptography.X509Certificates;

public class FloatingText : ScriptAction
{
    public override bool Finished => charCount == text.Length;
    public override bool CanBeSkipped => true;
    string text;
    private int screenWidth;
    private int screenHeight;
    int charCount;

    public FloatingText(string text)
    {
        this.text = text;
        screenWidth = GetScreenWidth();
        screenHeight = GetScreenHeight();
    }

    public override void Draw2D()
    {
        if (charCount < text.Length)
        {
            charCount++;
        }

        string visibleText = text.Substring(0, charCount);
        int textWidth = MeasureText(text, 30);

        int xPosition = (screenWidth - textWidth) / 2;
        int yPosition = (screenHeight * 2) / 3;

        DrawText(visibleText, xPosition, yPosition, 30, Color.White);
        base.Draw2D();
    }
    public override void Expedite()
    {
        charCount = text.Length;
    }
}