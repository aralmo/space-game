public struct PlanetGenerationSettings
{
    public (float height, Color color)[] HeightColors { get; set; }
    public NoiseType NoiseGenerationType { get; set; }
    public float HeightMultiplier { get; set; }
    public int IcoSphereSubdivisions {get;set;} = 3;
    public int MapTextureScale {get;set;} = 256;
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
            heightMap = GenImagePerlinNoise(settings.MapTextureScale, settings.MapTextureScale, (int)seed*settings.MapTextureScale, 0, 8.0f);
        }
        else
        {
            heightMap = GenImageCellular(settings.MapTextureScale, settings.MapTextureScale, 16);
        }
        Mesh planetMesh = Icosphere.GenerateIcospherePlanet(settings.IcoSphereSubdivisions, 1.0f, heightMap, settings.HeightColors, settings.HeightMultiplier);
        Model planetModel = LoadModelFromMesh(planetMesh);
        return planetModel;
    }
}