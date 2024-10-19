using System.ComponentModel;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

public class DialogAction : ScriptAction
{
    public override bool Finished => CharIndex == Text.Length;
    public override bool CanBeSkipped => true;
    public DialogSide DialogSide { get; }
    public string Text { get; protected set; }

    protected readonly Character Character;
    public DialogAction(DialogSide dialogSide, string characterId, string text)
    {
        DialogSide = dialogSide;
        Character = Characters.Get(characterId);
        Text = text.BreakUpLines(FONT_SIZE, GetScreenWidth() - 360);

        var w = GetScreenWidth() - 340;
        var h = 220;
        var img = BubbleImage(w, h, 4, 5, Color.Beige, new Color(200,200,200,200));
        bubble = LoadTextureFromImage(img);
    }
    protected int CharIndex = 0;
    protected Rectangle TextBubble;
    Texture2D bubble;
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
        var w = GetScreenWidth();
        var h = GetScreenHeight();
        float x = 0;
        if (DialogSide == DialogSide.left)
        {
            
            DrawTexture(Character.Portrait,0,h-300, Color.White);
            DrawTexture(bubble, 320, h-230, Color.White);
            x = 330;
        }
        else
        {
            DrawTexture(Character.Portrait,w-300,h-300, Color.White);
            DrawTexture(bubble, 20, h-230, Color.White);
            x = 30;
        }
        TextBubble = new Rectangle(x, h-210, w-340, 200);
    }
    public override void Expedite()
    {
        CharIndex = Text.Length;
    }
    Image BubbleImage(int width, int height, int borderPixels, int borderThickness, Color borderColor, Color fillColor)
    {
        Image image = GenImageColor(width, height, Color.Blank);
        height--;
        ImageDrawRectangle(ref image, borderPixels, borderPixels, width-borderPixels*2, height-borderPixels*2,fillColor);
        ImageDrawCircle(ref image, borderPixels, borderPixels, borderPixels, borderColor);
        ImageDrawCircle(ref image, width - borderPixels, borderPixels, borderPixels, borderColor);
        ImageDrawCircle(ref image, borderPixels, height - borderPixels, borderPixels, borderColor);
        ImageDrawCircle(ref image, width - borderPixels, height - borderPixels, borderPixels, borderColor);
        ImageDrawRectangle(ref image, borderPixels-1,0,width-borderPixels*2, borderPixels,borderColor);
        ImageDrawRectangle(ref image, borderPixels-1,height-borderPixels,width-borderPixels*2, borderPixels+1,borderColor);
        return image;
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