using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Scenes;

namespace Sindri.Physics2D.Components;

public sealed class Trigger2DComponent : Component
{
    private readonly Scene _scene;
    private readonly HashSet<Entity> _insideEntities = new();

    public Trigger2DComponent(Scene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        UpdateOrder = ComponentUpdateOrder.Triggers;
    }

    public string TargetTag { get; set; } = string.Empty;

    public event Action<Entity>? Entered;

    public event Action<Entity>? Stayed;

    public event Action<Entity>? Exited;

    public override void Update(SindriTime time)
    {
        if (Entity is null || !Entity.IsActive || Entity.IsDestroyed || string.IsNullOrWhiteSpace(TargetTag))
        {
            return;
        }

        var selfCollider = Entity.GetRequiredComponent<BoxCollider2D>();
        var overlappingThisFrame = new HashSet<Entity>();

        foreach (var target in _scene.FindEntitiesByTag(TargetTag))
        {
            if (Entity is null || Entity.IsDestroyed)
            {
                break;
            }

            if (target == Entity || target.IsDestroyed || !target.IsActive)
            {
                continue;
            }

            var targetCollider = target.GetComponent<BoxCollider2D>();

            if (targetCollider is null)
            {
                continue;
            }

            if (!selfCollider.Intersects(targetCollider))
            {
                continue;
            }

            overlappingThisFrame.Add(target);

            if (_insideEntities.Add(target))
            {
                Entered?.Invoke(target);
            }
            else
            {
                Stayed?.Invoke(target);
            }
        }

        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var exitedEntities = new List<Entity>();

        foreach (var entity in _insideEntities)
        {
            if (!overlappingThisFrame.Contains(entity))
            {
                exitedEntities.Add(entity);
            }
        }

        foreach (var entity in exitedEntities)
        {
            _insideEntities.Remove(entity);
            Exited?.Invoke(entity);
        }
    }

    protected override void OnDestroyed()
    {
        _insideEntities.Clear();
    }
}
