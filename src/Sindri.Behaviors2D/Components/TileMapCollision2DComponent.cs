using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Tilemaps;

namespace Sindri.Behaviors2D.Components;

public sealed class TileMapCollision2DComponent : Component
{
    private readonly TileMap2D _tileMap;
    private Vector2F _lastSafePosition;
    private bool _hasLastSafePosition;

    public TileMapCollision2DComponent(TileMap2D tileMap)
    {
        _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap)); UpdateOrder = ComponentUpdateOrder.Physics;
    }

    public TileMapCollision2DComponent(TileMap2D tileMap, float width, float height)
        : this(tileMap)
    {
        FallbackWidth = width;
        FallbackHeight = height;
    }

    public float FallbackWidth { get; set; } = 32f;

    public float FallbackHeight { get; set; } = 32f;

    public Vector2F MapWorldPosition { get; set; } = Vector2F.Zero;

    public bool UseAxisSeparation { get; set; } = true;

    public float MaxAxisResolveDistance { get; set; } = 128f;

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

        if (!IntersectsSolidAt(transform.Position))
        {
            _lastSafePosition = transform.Position;
            return;
        }

        if (UseAxisSeparation)
        {
            var delta = transform.Position - _lastSafePosition;
            var movedDistance = delta.Length;

            if (movedDistance <= MaxAxisResolveDistance)
            {
                var resolved = _lastSafePosition;

                var tryX = new Vector2F(transform.Position.X, _lastSafePosition.Y);

                if (!IntersectsSolidAt(tryX))
                {
                    resolved = tryX;
                }

                var tryY = new Vector2F(resolved.X, transform.Position.Y);

                if (!IntersectsSolidAt(tryY))
                {
                    resolved = tryY;
                }

                if (!IntersectsSolidAt(resolved))
                {
                    transform.Position = resolved;
                    _lastSafePosition = resolved;
                    return;
                }
            }
        }

        transform.Position = _lastSafePosition;
    }

    private bool IntersectsSolidAt(Vector2F position)
    {
        if (Entity is null)
        {
            return false;
        }

        var collider = Entity.GetComponent<BoxCollider2D>();

        var bounds = collider is not null
            ? collider.GetWorldBoundsAt(position)
            : new Rect2D(position.X, position.Y, FallbackWidth, FallbackHeight);

        return _tileMap.IntersectsSolid(bounds, MapWorldPosition);
    }
}
