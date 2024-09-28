public struct PlanetGenerationSettings
{
    public (float height, Color color)[] HeightColors { get; set; }
    public NoiseType NoiseGenerationType { get; set; }
    public float HeightMultiplier { get; set; }

    public PlanetGenerationSettings((float height, Color color)[] heightColors, NoiseType noiseGenerationType, float heightMultiplier)
    {
        HeightColors = heightColors;
        NoiseGenerationType = noiseGenerationType;
        HeightMultiplier = heightMultiplier;
    }
}

public enum NoiseType
{
    Perlin,
    Cellular
}

public static class PlanetGenerator
{
    public static unsafe Model GeneratePlanet(PlanetGenerationSettings settings, uint seed = 0)
    {
        Image heightMap;
        if (settings.NoiseGenerationType == NoiseType.Perlin)
        {
            heightMap = GenImagePerlinNoise(256, 256, (int)seed*256, 0, 8.0f);
        }
        else
        {
            heightMap = GenImageCellular(256, 256, 16);
        }
        Mesh planetMesh = Icosphere.GenerateIcospherePlanet(3, 1.0f, heightMap, settings.HeightColors, settings.HeightMultiplier);
        Model planetModel = LoadModelFromMesh(planetMesh);
        return planetModel;
    }
}