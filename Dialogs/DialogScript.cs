using System.Text.Json;

public class DialogScript
{
    public ScriptAction[] ScriptActions { get; private set; }
    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public DialogScript(string dialog)
    {
        string filename = $"gamedata/dialogs/{dialog}.json";
        var dialogData = LoadDialogData(filename);
        ScriptActions = ParseDialogData(dialogData);
    }

    private struct DialogEntry
    {
        public string Require { get; set; }
        public string Type { get; set; }
        public string? Text { get; set; }
        public DialogSide? Side { get; set; }
        public string? Who { get; set; }
        public ChoiceEntry[]? Choices { get; set; }
        public string? Param{get;set;}
        public string[]? If {get;set;}
    }

    private struct ChoiceEntry
    {
        public string? Set { get; set; }
        public string Text { get; set; }
    }

    private DialogEntry[] LoadDialogData(string filename)
    {
        string json = File.ReadAllText(filename);
        return JsonSerializer.Deserialize<DialogEntry[]>(json, serializerOptions);
    }

    private ScriptAction[] ParseDialogData(DialogEntry[] dialogData)
    {
        List<ScriptAction> actions = new List<ScriptAction>();

        foreach (var entry in dialogData)
        {
            switch (entry.Type)
            {
                case "floating":
                    actions.Add(new FloatingText(entry.Text)
                    {
                        If = entry.If
                    });
                    break;
                case "dialog":
                    actions.Add(new DialogAction(entry.Side.Value, entry.Who, entry.Text)
                    {
                        If = entry.If
                    });
                    break;
                case "choices":
                    var choices = new List<DialogChoice>();
                    foreach (var choice in entry.Choices)
                    {
                        choices.Add(new DialogChoice(choice.Text, choice.Set));
                    }
                    actions.Add(new DialogChoiceAction(entry.Who, choices.ToArray())
                    {
                        If = entry.If
                    });
                    break;
                case "camera":
                    actions.Add(new SetCameraAction(entry.Who, entry.Param)
                    {
                        If = entry.If
                    });
                    break;
            }
        }

        return actions.ToArray();
    }
}