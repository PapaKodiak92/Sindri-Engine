using Sindri.Graphics;

namespace Sindri.Behaviors2D.Components;

public sealed class TileHoverRenderer2D : RenderComponent
{
    private readonly TileHover2DComponent _hover;

    public TileHoverRenderer2D(TileHover2DComponent hover)
    {
        _hover = hover ?? throw new ArgumentNullException(nameof(hover));
        RenderLayer = 1_000;
        RenderOrder = 0;
    }

    public ColorRGBA HoverColor { get; set; } = ColorRGBA.SindriGold;

    public override void Render(IGraphicsDevice graphics)
    {
        if (!_hover.HasHoveredTile)
        {
            return;
        }

        var rect = _hover.HoveredTileWorldRect;

        graphics.FillRectangle(
            new Rect2D(
                rect.X + 8f,
                rect.Y + 8f,
                Math.Max(1f, rect.Width - 16f),
                Math.Max(1f, rect.Height - 16f)),
            HoverColor);
    }
}
