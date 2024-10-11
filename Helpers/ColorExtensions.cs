public static class ColorExtensions
{
    public static Color Lerp(this Color c1, Color c2, float amount)
    {
        float r = c1.R + (c2.R - c1.R) * amount;
        float g = c1.G + (c2.G - c1.G) * amount;
        float b = c1.B + (c2.B - c1.B) * amount;
        float a = c1.A + (c2.A - c1.A) * amount;
        return new Color((byte)r, (byte)g, (byte)b, (byte)a);
    }
}