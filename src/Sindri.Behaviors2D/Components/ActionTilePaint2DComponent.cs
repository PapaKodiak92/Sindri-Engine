using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Tilemaps;

namespace Sindri.Behaviors2D.Components;

public sealed class ActionTilePaint2DComponent : Component
{
    private readonly TileMap2D _tileMap;
    private readonly TileHover2DComponent _hover;
    private readonly InputActionMap _actions;

    public ActionTilePaint2DComponent(TileMap2D tileMap, TileHover2DComponent hover, InputActionMap actions)
    {
        _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
        _hover = hover ?? throw new ArgumentNullException(nameof(hover));
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
    }

    public string PaintAction { get; set; } = "PaintTile";

    public ColorRGBA SolidColor { get; set; } = ColorRGBA.SindriRed;

    public ColorRGBA WalkableColorA { get; set; } = new(32, 45, 58);

    public ColorRGBA WalkableColorB { get; set; } = new(24, 34, 46);

    public override void Update(SindriTime time)
    {
        if (!_hover.HasHoveredTile)
        {
            return;
        }

        if (!_actions.WasPressed(PaintAction))
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
