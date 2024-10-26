using static Raylib_cs.Raylib;

public abstract class UIItem
{
    public Size Width;
    public Size Height;

    protected UIItem(Size width, Size height)
    {
        Width = width;
        Height = height;
    }

    public abstract void Draw(int x, int y, int width, int height);
}
public abstract class UIContainer : UIItem
{
    public UIItem[] Children = Array.Empty<UIItem>();

    protected UIContainer(Size width, Size height) : base(width, height)
    {
    }
}
public class UIButton : UIItem
{
    private readonly Size width;
    private readonly Size height;
    private readonly string text;
    private readonly Action onClick;

    public UIButton(Size height, string text, Action onClick) : base(0, height)
    {
        this.text = text;
        this.onClick = onClick;
        this.height = height;
        this.width = Math.Max(50, MeasureText(text, 20) + 40); // Assuming 20 is the font size used for button text
        Width = width;
    }
    public UIButton(Size width, Size height, string text, Action onClick) : base(width, height)
    {
        this.width = width;
        this.height = height;
        this.text = text;
        this.onClick = onClick;
    }
    public override void Draw(int x, int y, int width, int height)
    {
        // Check if the mouse is over the button and if it is clicked
        Vector2 position = GetMousePosition();
        DrawRectangle(x, y, width, height, new Raylib_cs.Color(200, 200, 200, 255)); // Using a direct color definition instead
        if (position.X > x && position.X < x + width && position.Y > y && position.Y < y + height)
        {
            if (IsMouseButtonPressed(Raylib_cs.MouseButton.Left))
            {
                onClick.Invoke();
            }
            if (IsMouseButtonDown(Raylib_cs.MouseButton.Left))
            {
                DrawRectangle(x, y, width, height, new Raylib_cs.Color(100, 100, 100, 255)); // Darker shade for clicked button
            }
        }
        // Draw button text
        DrawText(text, x + (width / 2) - (MeasureText(text, 20) / 2), y + (height / 2) - 10, 20, Raylib_cs.Color.Black);
    }
}
public class UILabel : UIItem
{
    private readonly Size width;
    private readonly Size height;
    private readonly string text;

    public UILabel(Size height, string text) : base(0, height)
    {
        this.text = text;
        this.height = height;
        this.width = Math.Max(50, MeasureText(text, 20) + 40); // Assuming 20 is the font size used for label text
        Width = width;
    }
    public UILabel(Size width, Size height, string text) : base(width, height)
    {
        this.width = width;
        this.height = height;
        this.text = text;
    }
    public override void Draw(int x, int y, int width, int height)
    {
        // Draw label text
        DrawText(text, x + (width / 2) - (MeasureText(text, 20) / 2), y + (height / 2) - 10, 20, Raylib_cs.Color.Black);
    }
}
public class UIBar : UIItem
{
    private Color barColor;
    private Func<float> valueGetter;

    public UIBar(Size width, Size height, Color color, Func<float> valueGetter) : base(width, height)
    {
        this.barColor = color;
        this.valueGetter = valueGetter;
    }

    public override void Draw(int x, int y, int width, int height)
    {
        float value = Math.Clamp(valueGetter.Invoke(), 0f, 1f);
        int filledWidth = (int)(width * value);

        // Draw the background of the bar
        DrawRectangle(x, y, width, height, new Raylib_cs.Color(200, 200, 200, 255));
        // Draw the filled part of the bar
        DrawRectangle(x, y, filledWidth, height, barColor);
    }
}

public class UIPanel : UIContainer
{
    public int VerticalSpan = 10;
    public int HorizontalSpan = 10;
    public Color BackgroundColor = Color.Red;
    public UIPanel(Size width, Size height) : base(width, height) { }
    public bool AlignRight = false;
    public override void Draw(int x, int y, int width, int height)
    {
        int currentX = AlignRight ? x + width : x;
        int currentY = y + VerticalSpan;
        int maxYIncrease = 0; // Track the tallest component height in the current line

        DrawRectangle(x, y, width, height, BackgroundColor);
        foreach (var child in Children)
        {
            int childWidth = child.Width.GetSize(width);
            int childHeight = child.Height.GetSize(height);
            if (AlignRight)
            {
                if (currentX - childWidth < x) // Check if the child doesn't fit horizontally
                {
                    currentX = x + width; // Reset X to end position
                    currentY += maxYIncrease + VerticalSpan; // Increase Y by the tallest component in the previous line
                    maxYIncrease = 0; // Reset maxYIncrease for the next line
                }
                if (!this.Width.IsRelative)
                {
                    childWidth -= HorizontalSpan * 2;
                    currentX -= HorizontalSpan;
                }
                child.Draw(currentX - childWidth, currentY, childWidth, childHeight);
                currentX -= (childWidth + HorizontalSpan); // Move X to the left for the next child
            }
            else
            {
                if (currentX + childWidth > x + width) // Check if the child doesn't fit horizontally
                {
                    currentX = x; // Reset X to start position
                    currentY += maxYIncrease + VerticalSpan; // Increase Y by the tallest component in the previous line
                    maxYIncrease = 0; // Reset maxYIncrease for the next line
                }
                if (!this.Width.IsRelative)
                {
                    childWidth -= HorizontalSpan * 2;
                    currentX += HorizontalSpan;
                }
                child.Draw(currentX, currentY, childWidth, childHeight);
                currentX += (childWidth + HorizontalSpan); // Move X to the right for the next child
            }
            maxYIncrease = Math.Max(maxYIncrease, childHeight); // Update maxYIncrease if the current child is taller
        }
    }
}
public struct Size
{
    int? v;
    float? f;
    public bool IsRelative => f.HasValue;
    public Size(int value)
    {
        v = value;
    }
    public Size(float factor)
    {
        f = factor;
    }
    public int GetSize(int parentSize)
        => v.HasValue ? v.Value : (int)(parentSize * f!.Value);

    public static implicit operator Size(int value) => new Size(value);
    public static implicit operator Size(float factor) => new Size(factor);
}