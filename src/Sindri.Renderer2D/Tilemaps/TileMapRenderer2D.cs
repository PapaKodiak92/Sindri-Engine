using Sindri.Core.Entities;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Tilemaps;

public sealed class TileMapRenderer2D : RenderComponent
{
    public TileMapRenderer2D(TileMap2D tileMap)
    {
        TileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
    }

    public TileMap2D TileMap { get; }

    public int LastRenderedTileCount { get; private set; }

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var origin = transform.Position;
        var viewport = graphics.ViewportSize;
        var offset = graphics.DrawOffset;
        var scale = System.MathF.Max(0.0001f, graphics.DrawScale);

        var visibleLeft = -offset.X / scale;
        var visibleTop = -offset.Y / scale;
        var visibleRight = (viewport.Width - offset.X) / scale;
        var visibleBottom = (viewport.Height - offset.Y) / scale;

        var localVisibleLeft = visibleLeft - origin.X;
        var localVisibleTop = visibleTop - origin.Y;
        var localVisibleRight = visibleRight - origin.X;
        var localVisibleBottom = visibleBottom - origin.Y;

        var minTileX = System.Math.Clamp((int)System.MathF.Floor(localVisibleLeft / TileMap.TileSize), 0, TileMap.Width - 1);
        var minTileY = System.Math.Clamp((int)System.MathF.Floor(localVisibleTop / TileMap.TileSize), 0, TileMap.Height - 1);
        var maxTileX = System.Math.Clamp((int)System.MathF.Floor(localVisibleRight / TileMap.TileSize), 0, TileMap.Width - 1);
        var maxTileY = System.Math.Clamp((int)System.MathF.Floor(localVisibleBottom / TileMap.TileSize), 0, TileMap.Height - 1);

        LastRenderedTileCount = 0;

        for (var y = minTileY; y <= maxTileY; y++)
        {
            for (var x = minTileX; x <= maxTileX; x++)
            {
                var tile = TileMap.GetTile(x, y);

                graphics.FillRectangle(
                    new Rect2D(
                        origin.X + x * TileMap.TileSize,
                        origin.Y + y * TileMap.TileSize,
                        TileMap.TileSize,
                        TileMap.TileSize),
                    tile.Color);

                LastRenderedTileCount++;
            }
        }
    }
}
