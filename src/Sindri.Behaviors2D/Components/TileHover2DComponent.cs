using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Tilemaps;

namespace Sindri.Behaviors2D.Components;

public sealed class TileHover2DComponent : Component
{
    private readonly TileMap2D _tileMap;
    private readonly IMouseDevice _mouse;

    public TileHover2DComponent(TileMap2D tileMap, IMouseDevice mouse, Camera2D camera)
    {
        _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    public Camera2D Camera { get; }

    public Vector2F MapWorldPosition { get; set; } = Vector2F.Zero;

    public bool HasHoveredTile { get; private set; }

    public int HoveredTileX { get; private set; }

    public int HoveredTileY { get; private set; }

    public bool WasTileClicked { get; private set; }

    public int ClickedTileX { get; private set; }

    public int ClickedTileY { get; private set; }

    public Rect2D HoveredTileWorldRect { get; private set; }

    public override void Update(SindriTime time)
    {
        WasTileClicked = false;

        var mouseScreen = new Vector2F(_mouse.Position.X, _mouse.Position.Y);
        var mouseWorld = Camera.ScreenToWorld(mouseScreen);

        HasHoveredTile = _tileMap.TryWorldToTile(
            mouseWorld,
            MapWorldPosition,
            out var tileX,
            out var tileY);

        if (!HasHoveredTile)
        {
            return;
        }

        HoveredTileX = tileX;
        HoveredTileY = tileY;
        HoveredTileWorldRect = _tileMap.GetTileWorldRect(tileX, tileY, MapWorldPosition);

        if (_mouse.WasButtonPressed(MouseButton.Left))
        {
            WasTileClicked = true;
            ClickedTileX = tileX;
            ClickedTileY = tileY;
        }
    }
}
