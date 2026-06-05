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

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var origin = transform.Position;

        for (var y = 0; y < TileMap.Height; y++)
        {
            for (var x = 0; x < TileMap.Width; x++)
            {
                var color = TileMap.GetTile(x, y);

                graphics.FillRectangle(
                    new Rect2D(
                        origin.X + x * TileMap.TileSize,
                        origin.Y + y * TileMap.TileSize,
                        TileMap.TileSize,
                        TileMap.TileSize),
                    color);
            }
        }
    }
}
