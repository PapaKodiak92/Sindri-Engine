using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Tilemaps;

namespace Sindri.Behaviors2D.Components;

public sealed class DestroyOnSolidTile2DComponent : Component
{
    private readonly TileMap2D _tileMap;

    public DestroyOnSolidTile2DComponent(TileMap2D tileMap)
    {
        _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
    }

    public Vector2F MapWorldPosition { get; set; } = Vector2F.Zero;

    public float FallbackWidth { get; set; } = 8f;

    public float FallbackHeight { get; set; } = 8f;

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var collider = Entity.GetComponent<BoxCollider2D>();

        var bounds = collider is not null
            ? collider.GetWorldBounds()
            : new Rect2D(transform.Position.X, transform.Position.Y, FallbackWidth, FallbackHeight);

        if (_tileMap.IntersectsSolid(bounds, MapWorldPosition))
        {
            Entity.Destroy();
        }
    }
}
