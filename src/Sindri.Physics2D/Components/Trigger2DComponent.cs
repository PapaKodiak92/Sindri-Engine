using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Scenes;
using Sindri.Physics2D.Components;

namespace Sindri.Physics2D.Components;

public sealed class Trigger2DComponent : Component
{
    private readonly Scene _scene;

    public Trigger2DComponent(Scene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
    }

    public string TargetTag { get; set; } = string.Empty;

    public event Action<Entity>? Entered;

    public event Action<Entity>? Stayed;

    public override void Update(SindriTime time)
    {
        if (Entity is null || !Entity.IsActive)
        {
            return;
        }

        var selfCollider = Entity.GetRequiredComponent<BoxCollider2D>();

        foreach (var target in _scene.FindEntitiesByTag(TargetTag))
        {
            if (target == Entity)
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

            Entered?.Invoke(target);
            Stayed?.Invoke(target);
        }
    }
}
