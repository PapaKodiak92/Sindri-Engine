using System.Text.Json;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Tilemaps;

public static class TileMapJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static void Save(TileMap2D tileMap, string path)
    {
        ArgumentNullException.ThrowIfNull(tileMap);

        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var data = TileMapData.FromTileMap(tileMap);
        var json = JsonSerializer.Serialize(data, Options);

        File.WriteAllText(path, json);
    }

    public static TileMap2D Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Tilemap file was not found.", path);
        }

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<TileMapData>(json, Options)
            ?? throw new InvalidOperationException("Failed to deserialize tilemap data.");

        return data.ToTileMap();
    }

    private sealed class TileMapData
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public int TileSize { get; set; }

        public List<TileData> Tiles { get; set; } = new();

        public static TileMapData FromTileMap(TileMap2D tileMap)
        {
            var data = new TileMapData
            {
                Width = tileMap.Width,
                Height = tileMap.Height,
                TileSize = tileMap.TileSize
            };

            for (var y = 0; y < tileMap.Height; y++)
            {
                for (var x = 0; x < tileMap.Width; x++)
                {
                    var tile = tileMap.GetTile(x, y);

                    data.Tiles.Add(new TileData
                    {
                        R = tile.Color.R,
                        G = tile.Color.G,
                        B = tile.Color.B,
                        A = tile.Color.A,
                        IsSolid = tile.IsSolid
                    });
                }
            }

            return data;
        }

        public TileMap2D ToTileMap()
        {
            if (Tiles.Count != Width * Height)
            {
                throw new InvalidOperationException("Tilemap data tile count does not match width and height.");
            }

            var tileMap = new TileMap2D(Width, Height, TileSize);

            var index = 0;

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var tile = Tiles[index++];

                    tileMap.SetTile(
                        x,
                        y,
                        new Tile2D(
                            new ColorRGBA(tile.R, tile.G, tile.B, tile.A),
                            tile.IsSolid));
                }
            }

            return tileMap;
        }
    }

    private sealed class TileData
    {
        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        public byte A { get; set; } = 255;

        public bool IsSolid { get; set; }
    }
}
