using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Components;

public sealed class TextRenderer2D : RenderComponent
{
    public TextRenderer2D(string text, ColorRGBA color)
    {
        Text = text;
        Color = color;
        RenderSpace = RenderSpace.Screen;
        RenderLayer = 10_000;
    }

    public string Text { get; set; }

    public ColorRGBA Color { get; set; }

    public Vector2F Offset { get; set; } = Vector2F.Zero;

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        graphics.DrawText(Text, transform.Position + Offset, Color);
    }
}
