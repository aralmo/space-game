using System.Text.Json;

public static class Characters
{
    private static readonly Dictionary<string, Character> charactersById;

    static Characters()
    {
        string jsonFilePath = "gamedata/characters.json";
        string jsonData = File.ReadAllText(jsonFilePath);
        var characterList = JsonSerializer.Deserialize<List<CharacterData>>(jsonData);

        charactersById = characterList?.ToDictionary(
            character => character.Id,
            character => new Character(character.Name, character.Portrait)
        ) ?? throw new Exception("Error loading characters");
    }

    public static Character Get(string id)
    {
        if (charactersById.TryGetValue(id, out var character))
        {
            return character;
        }
        throw new KeyNotFoundException($"Character with ID '{id}' not found.");
    }


    private class CharacterData
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Portrait { get; set; }
    }
}