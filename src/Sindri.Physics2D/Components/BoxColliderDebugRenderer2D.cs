using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Physics2D.Components;

public sealed class BoxColliderDebugRenderer2D : RenderComponent
{
    public BoxColliderDebugRenderer2D()
    {
        RenderLayer = 50_000;
        IsVisible = false;
    }

    public ColorRGBA Color { get; set; } = ColorRGBA.SindriCyan;

    public float Thickness { get; set; } = 2f;

    public Vector2F Padding { get; set; } = Vector2F.Zero;

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var collider = Entity.GetComponent<BoxCollider2D>();

        if (collider is null)
        {
            return;
        }

        var bounds = collider.GetWorldBounds();

        var x = bounds.X - Padding.X;
        var y = bounds.Y - Padding.Y;
        var width = bounds.Width + Padding.X * 2f;
        var height = bounds.Height + Padding.Y * 2f;

        if (width <= 0f || height <= 0f)
        {
            return;
        }

        var thickness = System.MathF.Max(1f, System.MathF.Min(Thickness, System.MathF.Min(width, height)));

        graphics.FillRectangle(new Rect2D(x, y, width, thickness), Color);
        graphics.FillRectangle(new Rect2D(x, y + height - thickness, width, thickness), Color);
        graphics.FillRectangle(new Rect2D(x, y, thickness, height), Color);
        graphics.FillRectangle(new Rect2D(x + width - thickness, y, thickness, height), Color);
    }
}
