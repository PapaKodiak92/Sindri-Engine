using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Components;

public sealed class RectangleRenderer2D : RenderComponent
{
    public RectangleRenderer2D(float width, float height, ColorRGBA color)
    {
        Width = width;
        Height = height;
        Color = color;
    }

    public float Width { get; set; }

    public float Height { get; set; }

    public ColorRGBA Color { get; set; }

    public bool ClampToViewport { get; set; } = true;

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var position = transform.Position;

        if (ClampToViewport)
        {
            var viewport = graphics.ViewportSize;

            position = new Vector2F(
                X: Math.Clamp(position.X, 0f, Math.Max(0f, viewport.Width - Width)),
                Y: Math.Clamp(position.Y, 0f, Math.Max(0f, viewport.Height - Height)));

            transform.Position = position;
        }

        graphics.FillRectangle(
            new Rect2D(position.X, position.Y, Width, Height),
            Color);
    }
}
