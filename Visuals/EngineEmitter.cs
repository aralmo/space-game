public class EngineEmitter : Transform, I3DDrawable, IUpdatable
{
    private List<Particle> particles;
    private float particleSize;
    private readonly Color particleStartColor;
    private readonly Color particleEndColor;
    private int maxParticles;
    private float emitRate;
    private double lastEmitTime;
    public Vector3 Direction = Vector3.Zero;
    private Model model;

    public EngineEmitter(float particleSize = .085f, Color? particleStartColor = null, Color? particleEndColor = null, int maxParticles = 20, float emitRate = 0.002f)
    {
        this.particles = new List<Particle>();
        this.particleSize = particleSize;
        this.particleStartColor = particleStartColor ?? new Color(41, 166, 207, 255);
        this.particleEndColor = particleEndColor ?? Color.DarkBlue;
        this.maxParticles = maxParticles;
        this.emitRate = emitRate;
        this.lastEmitTime = GetTime();
        Direction = Position.Normalize();
        model = LoadModelFromMesh(GenMeshCube(1,1,1));
    }

    public void Clear()
    {
        this.particles.Clear();
    }
    public void Update()
    {
        double currentTime = GetTime();
        if (currentTime - lastEmitTime >= emitRate && particles.Count < maxParticles)
        {
            EmitParticle();
            lastEmitTime = currentTime;
        }

        foreach (var particle in particles)
        {
            particle.Update();
        }

        particles.RemoveAll(p => p.Lifetime <= 0);
    }

    private void EmitParticle()
    {
        var position = Position;
        var velocity = Direction * .02f * Scale;
        var f = 0.00015f * Scale;
        var pv = velocity + new Vector3(Random.Shared.Next(-10, 10) * f, Random.Shared.Next(-10, 10) * f, Random.Shared.Next(-10, 10) * f);
        particles.Add(new Particle(position, particleSize, particleStartColor, pv));
    }

    public void Draw3D()
    {
        var matrix = GetWorldMatrix();
        foreach (var particle in particles)
        {
            //todo: draw instanced!
            var p = particleSize * particle.Lifetime;
            Color lerpedColor = particleStartColor.Lerp(particleEndColor, particle.Traveled / 2f);        
            model.Transform = matrix;            
            DrawModel(model,TransformPointByParent(particle.Position),p,lerpedColor);
        }
    }

    private class Particle
    {
        public Vector3 Position { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }
        public float Lifetime { get; set; }
        public float Traveled { get; set; }
        private Vector3 Velocity;

        public Particle(Vector3 position, float size, Color color, Vector3 velocity)
        {
            this.Position = position;
            this.Size = size;
            this.Color = color;
            this.Lifetime = Random.Shared.Next(100, 150) * .01f; // Example lifetime
            this.Velocity = velocity;
        }

        public void Update()
        {
            Position += Velocity;
            Traveled += Velocity.Magnitude();
            Lifetime -= 0.04f; // Example decrease in lifetime
        }
    }
}