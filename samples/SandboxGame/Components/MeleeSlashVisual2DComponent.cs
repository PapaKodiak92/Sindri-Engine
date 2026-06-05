using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

internal sealed class MeleeSlashVisual2DComponent : RenderComponent
{
    private float _ageSeconds;

    public MeleeSlashVisual2DComponent(ColorRGBA color)
    {
        Color = color;
        RenderLayer = 25;
        UpdateOrder = ComponentUpdateOrder.Lifetime;
    }

    public ColorRGBA Color { get; set; }

    public Vector2F Direction { get; set; } = new(1f, 0f);

    public float Length { get; set; } = 64f;

    public float Thickness { get; set; } = 12f;

    public float LifetimeSeconds { get; set; } = 0.10f;

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        _ageSeconds += time.DeltaSeconds;

        if (_ageSeconds >= LifetimeSeconds)
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
        var direction = Direction.Normalized();

        if (direction == Vector2F.Zero)
        {
            direction = new Vector2F(1f, 0f);
        }

        var perpendicular = new Vector2F(-direction.Y, direction.X);

        var segments = 5;
        var segmentLength = Length / segments;

        for (var i = 0; i < segments; i++)
        {
            var t = i / (float)(segments - 1);
            var size = Thickness * (1f - t * 0.45f);

            var center =
                transform.Position +
                direction * (i * segmentLength) +
                perpendicular * System.MathF.Sin(t * System.MathF.PI) * 18f;

            graphics.FillRectangle(
                new Rect2D(
                    center.X - size / 2f,
                    center.Y - size / 2f,
                    size,
                    size),
                Color);
        }
    }
}
