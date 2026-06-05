using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Behaviors2D.Components;

public sealed class FloatingText2DComponent : RenderComponent
{
    private float _elapsedSeconds;

    public FloatingText2DComponent(string text, ColorRGBA color)
    {
        Text = text;
        Color = color;
        RenderLayer = 10_000;
    }

    public string Text { get; set; }

    public ColorRGBA Color { get; set; }

    public Vector2F Velocity { get; set; } = new(0f, -48f);

    public float LifetimeSeconds { get; set; } = 0.75f;

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        _elapsedSeconds += time.DeltaSeconds;

        var transform = Entity.GetRequiredComponent<Transform2D>();
        transform.Position += Velocity * time.DeltaSeconds;

        if (_elapsedSeconds >= LifetimeSeconds)
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
        graphics.DrawText(Text, transform.Position, Color);
    }
}
