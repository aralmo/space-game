

public static class PlanetSettings
{
    public static readonly PlanetGenerationSettings YellowSun = new PlanetGenerationSettings(
    new (float height, Color color)[]
    {
            (0.4f, new Color(255, 255, 0, 255)),   // Full Yellow
            (0.6f, new Color(255, 245, 0, 255)),   // Bright Yellow
            (0.85f, new Color(255, 235, 0, 255)),  // Pale Yellow
            (1.0f, new Color(255, 225, 0, 255))    // Orange (Orange)
    },
    NoiseType.Perlin,
    0f
    );

    public static readonly PlanetGenerationSettings EarthLike = new PlanetGenerationSettings(
        new (float height, Color color)[]
        {
            (0.4f, Color.Blue),   // Water
            (0.6f, Color.Brown),  // Rock
            (1.0f, Color.DarkGreen) // Dirt
        },
        NoiseType.Perlin,
        0.15f
    );

    public static readonly PlanetGenerationSettings Moon = new PlanetGenerationSettings(
        new (float height, Color color)[]
        {
            (0.4f, Color.Gray),   // Lowlands
            (0.6f, Color.LightGray),  // Highlands
            (1.0f, Color.DarkGray) // Craters
        },
        NoiseType.Cellular,
        0.1f
    );

    public static readonly PlanetGenerationSettings MarsLike = new PlanetGenerationSettings(
        new (float height, Color color)[]
        {
            (0.4f, new Color(139, 69, 19, 255)),   // Lowlands (SaddleBrown)
            (0.6f, new Color(205, 92, 92, 255)),  // Highlands (IndianRed)
            (1.0f, new Color(139, 0, 0, 255)) // Peaks (DarkRed)
        },
        NoiseType.Perlin,
        0.2f
    );
    public static readonly PlanetGenerationSettings VenusLike = new PlanetGenerationSettings(
        new (float height, Color color)[]
        {
            (0.4f, new Color(255, 165, 0, 255)),   // Lowlands (Orange)
            (0.6f, new Color(255, 255, 0, 255)),  // Highlands (Yellow)
            (1.0f, new Color(255, 255, 224, 255)) // Peaks (LightYellow)
        },
        NoiseType.Cellular,
        0.12f
    );

    public static readonly PlanetGenerationSettings IcePlanet = new PlanetGenerationSettings(
        new (float height, Color color)[]
        {
            (0.4f, new Color(173, 216, 230, 255)),   // Ice (LightBlue)
            (0.6f, new Color(255, 255, 255, 255)),  // Snow (White)
            (1.0f, new Color(128, 128, 128, 255)) // Rocky Peaks (Gray)
        },
        NoiseType.Perlin,
        0.1f
    );

    public static readonly PlanetGenerationSettings LavaPlanet = new PlanetGenerationSettings(
        new (float height, Color color)[]
        {
            (0.2f, new Color(139, 0, 0, 255)),   // Lava (DarkRed)
            (0.3f, new Color(255, 0, 0, 255)),  // Molten Rock (Red)
            (0.35f, new Color(69, 36, 15, 255)), // mid
            (1.0f, new Color(34, 18, 7, 255)) // Peaks (Darker Volcano Dirt)
        },
        NoiseType.Cellular,
        0.25f
    );
}
