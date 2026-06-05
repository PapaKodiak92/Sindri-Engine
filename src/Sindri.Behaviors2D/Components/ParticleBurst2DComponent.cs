using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Behaviors2D.Components;

public sealed class ParticleBurst2DComponent : RenderComponent
{
    private readonly Particle[] _particles;
    private bool _initialized;

    public ParticleBurst2DComponent(int particleCount, ColorRGBA color)
    {
        if (particleCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(particleCount));
        }

        _particles = new Particle[particleCount];
        Color = color;
        RenderLayer = 35;
        UpdateOrder = ComponentUpdateOrder.Lifetime;
    }

    public ColorRGBA Color { get; set; }

    public float MinSpeed { get; set; } = 40f;

    public float MaxSpeed { get; set; } = 160f;

    public float MinLifetimeSeconds { get; set; } = 0.25f;

    public float MaxLifetimeSeconds { get; set; } = 0.65f;

    public float MinSize { get; set; } = 3f;

    public float MaxSize { get; set; } = 7f;

    protected override void OnAttached()
    {
        InitializeParticles();
    }

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        if (!_initialized)
        {
            InitializeParticles();
        }

        var anyAlive = false;

        foreach (var particle in _particles)
        {
            if (particle.AgeSeconds >= particle.LifetimeSeconds)
            {
                continue;
            }

            particle.AgeSeconds += time.DeltaSeconds;
            particle.Position += particle.Velocity * time.DeltaSeconds;

            if (particle.AgeSeconds < particle.LifetimeSeconds)
            {
                anyAlive = true;
            }
        }

        if (!anyAlive)
        {
            Entity.Destroy();
        }
    }

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();

        foreach (var particle in _particles)
        {
            if (particle.AgeSeconds >= particle.LifetimeSeconds)
            {
                continue;
            }

            graphics.FillRectangle(
                new Rect2D(
                    transform.Position.X + particle.Position.X,
                    transform.Position.Y + particle.Position.Y,
                    particle.Size,
                    particle.Size),
                Color);
        }
    }

    private void InitializeParticles()
    {
        for (var i = 0; i < _particles.Length; i++)
        {
            var angle = RandomRange(0f, System.MathF.PI * 2f);
            var speed = RandomRange(MinSpeed, MaxSpeed);

            _particles[i] = new Particle
            {
                Position = Vector2F.Zero,
                Velocity = new Vector2F(
                    System.MathF.Cos(angle) * speed,
                    System.MathF.Sin(angle) * speed),
                LifetimeSeconds = RandomRange(MinLifetimeSeconds, MaxLifetimeSeconds),
                Size = RandomRange(MinSize, MaxSize)
            };
        }

        _initialized = true;
    }

    private static float RandomRange(float min, float max)
    {
        return min + Random.Shared.NextSingle() * (max - min);
    }

    private sealed class Particle
    {
        public Vector2F Position { get; set; }

        public Vector2F Velocity { get; set; }

        public float AgeSeconds { get; set; }

        public float LifetimeSeconds { get; set; }

        public float Size { get; set; }
    }
}
