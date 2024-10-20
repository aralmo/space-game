using System.Text.Json;

public class Vessel : Transform
{
    public string[] Animations { get; set; }
    Transform[] transforms;
    private Vessel() { }
    public static Vessel LoadFromFile(string file)
    {
        List<Transform> transforms = new();
        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IncludeFields = true };
        var data = JsonSerializer.Deserialize<ShipDataModel>(File.ReadAllText(file), options);
        var vessel = new Vessel();
        foreach (var v in data.Visuals)
        {
            if (!string.IsNullOrEmpty(v.Model))
            {
                ShipAnimation[] anims = Array.Empty<ShipAnimation>();
                if (v.Animations.Any())
                {
                    anims = LoadAnimations(v).ToArray();
                }
                transforms.Add(new ModelTransform()
                {
                    Model = ShipModels.Load(v.Model),
                    Position = v.Position,
                    Parent = vessel,
                    Animations = anims
                });
                continue;
            }
            if (!string.IsNullOrEmpty(v.Effect))
            {
                switch (v.Effect)
                {
                    case "engine":
                        transforms.Add(new EngineEmitter()
                        {
                            Position = v.Position,
                            Parent = vessel
                        }); break;
                }
            }
        }

        vessel.Scale = data.Scale;
        vessel.transforms = transforms.ToArray();
        return vessel;
    }
    static IEnumerable<ShipAnimation> LoadAnimations(VisualModel v)
    {
        if (v.Animations.Any())
        {
            var a = ShipModels.LoadAnimations(v.Model);
            for (int i = 0; i < v.Animations.Length; i++)
            {
                yield return new ShipAnimation()
                {
                    Frames = v.Animations[i].Frames,
                    ModelAnimation = a[i],
                    Name = v.Animations[i].Name,
                    Playing = true
                };
            }
        }
    }

    public void Draw3D()
    {
        foreach (I3DDrawable d in transforms.Where(t => t is I3DDrawable))
        {
            d.Draw3D();
        }
    }
    public void Update()
    {
        foreach (IUpdatable t in transforms.Where(t => t is IUpdatable))
        {
            t.Update();
        }
    }
    public void SwitchAnimation(string animation)
    {
        foreach (ModelTransform mt in transforms.Where(t => t is ModelTransform))
        {
            var match = mt.Animations.FirstOrDefault(a => a.Name == animation);
            if (match != null)
            {
                match.Playing = !match.Playing;
            }
        }
    }
    private class ShipDataModel
    {
        public string Name { get; set; }
        public string Maker { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public float Scale { get; set; }
        public VisualModel[] Visuals { get; set; }
    }
    private class VisualModel
    {
        public Vector3 Position { get; set; }
        public string Model { get; set; }
        public AnimationModel[] Animations { get; set; }
        public string Effect { get; set; }
    }
    private class AnimationModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Frames { get; set; }
    }
}