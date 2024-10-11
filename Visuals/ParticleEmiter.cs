using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Raylib_cs.Raylib;

public class ParticleEmitter
{
    private List<Particle> particles;
    public Vector3 position;
    public Vector3 velocity;
    private float particleSize;
    private readonly Color particleStartColor;
    private readonly Color particleEndColor;
    private int maxParticles;
    private float emitRate;
    private double lastEmitTime;

    public ParticleEmitter(Vector3 position, Vector3 velocity, float particleSize, Color particleStartColor, Color particleEndColor, int maxParticles, float emitRate)
    {
        this.particles = new List<Particle>();
        this.position = position;
        this.velocity = velocity;
        this.particleSize = particleSize;
        this.particleStartColor = particleStartColor;
        this.particleEndColor = particleEndColor;
        this.maxParticles = maxParticles;
        this.emitRate = emitRate;
        this.lastEmitTime = GetTime();
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
        var pv = velocity + new Vector3(Random.Shared.Next(-10, 10) * .00008f, Random.Shared.Next(-10, 10) * .00008f, Random.Shared.Next(-10, 10) * .00008f);
        particles.Add(new Particle(position, particleSize, particleStartColor, pv));
    }

    public void Draw3D()
    {
        foreach (var particle in particles)
        {
            var p = particleSize * particle.Lifetime;
            Color lerpedColor = particleStartColor.Lerp(particleEndColor, particle.Traveled / 2f);
            DrawCube(particle.Position, p, p, p, lerpedColor);
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