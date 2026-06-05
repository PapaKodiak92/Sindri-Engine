using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Scenes;

namespace Sindri.Physics2D.Components;

public sealed class EntityCollision2DComponent : Component
{
    private readonly Scene _scene;
    private Vector2F _lastSafePosition;
    private bool _hasLastSafePosition;

    public EntityCollision2DComponent(Scene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        UpdateOrder = ComponentUpdateOrder.Physics + 100;
    }

    public string TargetTag { get; set; } = "Solid";

    public bool UseAxisSeparation { get; set; } = true;

    public float MaxAxisResolveDistance { get; set; } = 128f;

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
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

            if (delta.Length <= MaxAxisResolveDistance)
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

        var selfCollider = Entity.GetComponent<BoxCollider2D>();

        if (selfCollider is null)
        {
            return false;
        }

        var selfBounds = selfCollider.GetWorldBoundsAt(position);

        foreach (var target in _scene.FindEntitiesByTag(TargetTag))
        {
            if (target == Entity || target.IsDestroyed || !target.IsActive)
            {
                continue;
            }

            var targetCollider = target.GetComponent<BoxCollider2D>();

            if (targetCollider is null || targetCollider.IsTrigger)
            {
                continue;
            }

            if (BoxCollider2D.Intersects(selfBounds, targetCollider.GetWorldBounds()))
            {
                return true;
            }
        }

        return false;
    }
}
