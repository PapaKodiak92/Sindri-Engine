using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Tilemaps;

public sealed class TileMap2D
{
    private readonly Tile2D[] _tiles;

    public TileMap2D(int width, int height, int tileSize)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (tileSize <= 0) throw new ArgumentOutOfRangeException(nameof(tileSize));

        Width = width;
        Height = height;
        TileSize = tileSize;

        _tiles = new Tile2D[width * height];

        Fill(new Tile2D(ColorRGBA.SindriBlue));
    }

    public int Width { get; }

    public int Height { get; }

    public int TileSize { get; }

    public Tile2D GetTile(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _tiles[ToIndex(x, y)];
    }

    public void SetTile(int x, int y, ColorRGBA color)
    {
        SetTile(x, y, new Tile2D(color));
    }

    public void SetTile(int x, int y, ColorRGBA color, bool isSolid)
    {
        SetTile(x, y, new Tile2D(color, isSolid));
    }

    public void SetTile(int x, int y, Tile2D tile)
    {
        ValidateCoordinates(x, y);
        _tiles[ToIndex(x, y)] = tile;
    }

    public void Fill(ColorRGBA color)
    {
        Fill(new Tile2D(color));
    }

    public void Fill(Tile2D tile)
    {
        Array.Fill(_tiles, tile);
    }

    public void CopyFrom(TileMap2D other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other.Width != Width || other.Height != Height || other.TileSize != TileSize)
        {
            throw new InvalidOperationException("Cannot copy tilemap data from a map with different dimensions or tile size.");
        }

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                SetTile(x, y, other.GetTile(x, y));
            }
        }
    }

    public bool ContainsTile(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool TryWorldToTile(Vector2F worldPosition, Vector2F mapWorldPosition, out int tileX, out int tileY)
    {
        var localX = worldPosition.X - mapWorldPosition.X;
        var localY = worldPosition.Y - mapWorldPosition.Y;

        tileX = (int)MathF.Floor(localX / TileSize);
        tileY = (int)MathF.Floor(localY / TileSize);

        return ContainsTile(tileX, tileY);
    }

    public Rect2D GetTileWorldRect(int x, int y, Vector2F mapWorldPosition)
    {
        ValidateCoordinates(x, y);

        return new Rect2D(
            mapWorldPosition.X + x * TileSize,
            mapWorldPosition.Y + y * TileSize,
            TileSize,
            TileSize);
    }

    public bool IntersectsSolid(Rect2D worldRect, Vector2F mapWorldPosition)
    {
        var localLeft = worldRect.X - mapWorldPosition.X;
        var localTop = worldRect.Y - mapWorldPosition.Y;
        var localRight = localLeft + worldRect.Width;
        var localBottom = localTop + worldRect.Height;

        var minTileX = (int)MathF.Floor(localLeft / TileSize);
        var minTileY = (int)MathF.Floor(localTop / TileSize);
        var maxTileX = (int)MathF.Floor((localRight - 1f) / TileSize);
        var maxTileY = (int)MathF.Floor((localBottom - 1f) / TileSize);

        for (var y = minTileY; y <= maxTileY; y++)
        {
            for (var x = minTileX; x <= maxTileX; x++)
            {
                if (!ContainsTile(x, y))
                {
                    continue;
                }

                if (GetTile(x, y).IsSolid)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private int ToIndex(int x, int y)
    {
        return y * Width + x;
    }

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= Width)
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }

        if (y < 0 || y >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }
    }
}
