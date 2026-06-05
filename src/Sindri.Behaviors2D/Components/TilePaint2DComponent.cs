using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Tilemaps;

namespace Sindri.Behaviors2D.Components;

public sealed class TilePaint2DComponent : Component
{
    private readonly TileMap2D _tileMap;
    private readonly TileHover2DComponent _hover;
    private readonly IMouseDevice _mouse;

    public TilePaint2DComponent(TileMap2D tileMap, TileHover2DComponent hover, IMouseDevice mouse)
    {
        _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
        _hover = hover ?? throw new ArgumentNullException(nameof(hover));
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
    }

    public MouseButton PaintButton { get; set; } = MouseButton.Right;

    public ColorRGBA SolidColor { get; set; } = ColorRGBA.SindriRed;

    public ColorRGBA WalkableColorA { get; set; } = new(32, 45, 58);

    public ColorRGBA WalkableColorB { get; set; } = new(24, 34, 46);

    public override void Update(SindriTime time)
    {
        if (!_hover.HasHoveredTile)
        {
            return;
        }

        if (!_mouse.WasButtonPressed(PaintButton))
        {
            return;
        }

        var x = _hover.HoveredTileX;
        var y = _hover.HoveredTileY;
        var tile = _tileMap.GetTile(x, y);

        if (tile.IsSolid)
        {
            var checker = (x + y) % 2 == 0;
            _tileMap.SetTile(x, y, checker ? WalkableColorA : WalkableColorB, isSolid: false);
        }
        else
        {
            _tileMap.SetTile(x, y, SolidColor, isSolid: true);
        }
    }
}
