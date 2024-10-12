
public class PlayerShip
{
    private readonly Model model;
    private readonly ModelAnimation[] animations;
    HashSet<string> playingEffects = new HashSet<string>();
    public Simulation Simulation { get; private set; }
    public DynamicSimulation DynamicSimulation { get; private set; }

    private EngineEmitter engineParticles;
    public bool enginePlaying = true;
    public unsafe PlayerShip(Simulation simulation, DynamicSimulation dynamicSimulation, string model)
    {
        Simulation = simulation;
        this.model = ShipModels.Load(model);
        this.animations = ShipModels.LoadAnimations(model);
        var fc = animations[0].FrameCount;
        DynamicSimulation = dynamicSimulation;
        engineParticles = new EngineEmitter(
            dynamicSimulation,
            particleSize: .08f,
            particleStartColor: new Color(41, 166, 207, 255),
            particleEndColor: Color.DarkBlue,
            maxParticles: 20,
            emitRate: 0.002f);
    }
    public void Update()
    {
        UpdateEffects();
    }
    int hangarFrame;

    private void UpdateEffects()
    {
        if (playingEffects.Contains("hangar"))
        {
            if (hangarFrame < animations[0].FrameCount - 10)
            {
                hangarFrame++;
                UpdateModelAnimation(model, animations[0], hangarFrame);
            }
        }
        else
        {
            if (hangarFrame > 0)
            {
                hangarFrame++;
                UpdateModelAnimation(model, animations[0], hangarFrame);
            }
        }
        if (enginePlaying)
        {
            engineParticles.Update();
        }
    }
    public void Draw3D()
    {
        DynamicSimulation.Draw3D(model);
        if (enginePlaying) engineParticles.Draw3D();
    }
    public void PlayEffect(string effect)
    {
        playingEffects.Add(effect);
    }
    public void StopEffect(string effect)
    {
        playingEffects.Add(effect);
    }
}
