using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Renderer2D.Tilemaps;

namespace Sindri.Behaviors2D.Components;

public sealed class TileMapCollision2DComponent : Component
{
    private readonly TileMap2D _tileMap;
    private Vector2F _lastSafePosition;
    private bool _hasLastSafePosition;

    public TileMapCollision2DComponent(TileMap2D tileMap, float width, float height)
    {
        _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
        Width = width;
        Height = height;
    }

    public float Width { get; set; }

    public float Height { get; set; }

    public Vector2F MapWorldPosition { get; set; } = Vector2F.Zero;

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();

        if (!_hasLastSafePosition)
        {
            _lastSafePosition = transform.Position;
            _hasLastSafePosition = true;
        }

        var bounds = new Rect2D(
            transform.Position.X,
            transform.Position.Y,
            Width,
            Height);

        if (_tileMap.IntersectsSolid(bounds, MapWorldPosition))
        {
            transform.Position = _lastSafePosition;
            return;
        }

        _lastSafePosition = transform.Position;
    }
}
