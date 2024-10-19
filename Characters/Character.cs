public class Character
{
    private readonly string portraitPath;

    public string Name { get; }
    public Texture2D Portrait => Portraits.Get(portraitPath); 
    public Character(string name, string portrait)
    {
        Name = name;
        this.portraitPath = portrait;
    }
}