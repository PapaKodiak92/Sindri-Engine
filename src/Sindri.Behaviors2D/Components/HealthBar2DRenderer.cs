using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Behaviors2D.Components;

public sealed class HealthBar2DRenderer : RenderComponent
{
    private readonly Health2DComponent _health;

    public HealthBar2DRenderer(Health2DComponent health)
    {
        _health = health ?? throw new ArgumentNullException(nameof(health));
        RenderLayer = 30;
    }

    public float Width { get; set; } = 48f;

    public float Height { get; set; } = 6f;

    public Vector2F Offset { get; set; } = new(0f, -12f);

    public ColorRGBA BackgroundColor { get; set; } = new(32, 12, 12);

    public ColorRGBA FillColor { get; set; } = ColorRGBA.SindriGreen;

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var healthPercent = _health.MaxHealth <= 0
            ? 0f
            : (float)_health.CurrentHealth / _health.MaxHealth;

        healthPercent = Math.Clamp(healthPercent, 0f, 1f);

        var x = transform.Position.X + Offset.X;
        var y = transform.Position.Y + Offset.Y;

        graphics.FillRectangle(
            new Rect2D(x, y, Width, Height),
            BackgroundColor);

        if (healthPercent > 0f)
        {
            graphics.FillRectangle(
                new Rect2D(x, y, Width * healthPercent, Height),
                FillColor);
        }
    }
}
