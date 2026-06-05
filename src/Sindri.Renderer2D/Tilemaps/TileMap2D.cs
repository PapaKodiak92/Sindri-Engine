using Sindri.Graphics;

namespace Sindri.Renderer2D.Tilemaps;

public sealed class TileMap2D
{
    private readonly ColorRGBA[] _tiles;

    public TileMap2D(int width, int height, int tileSize)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (tileSize <= 0) throw new ArgumentOutOfRangeException(nameof(tileSize));

        Width = width;
        Height = height;
        TileSize = tileSize;

        _tiles = new ColorRGBA[width * height];

        Fill(ColorRGBA.SindriBlue);
    }

    public int Width { get; }

    public int Height { get; }

    public int TileSize { get; }

    public ColorRGBA GetTile(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _tiles[ToIndex(x, y)];
    }

    public void SetTile(int x, int y, ColorRGBA color)
    {
        ValidateCoordinates(x, y);
        _tiles[ToIndex(x, y)] = color;
    }

    public void Fill(ColorRGBA color)
    {
        Array.Fill(_tiles, color);
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
