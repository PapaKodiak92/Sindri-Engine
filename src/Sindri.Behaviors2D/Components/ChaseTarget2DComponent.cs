using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;

namespace Sindri.Behaviors2D.Components;

public sealed class ChaseTarget2DComponent : Component
{
    private readonly Transform2D _target;

    public ChaseTarget2DComponent(Transform2D target, float speed)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        Speed = speed; UpdateOrder = ComponentUpdateOrder.Movement;
    }

    public float Speed { get; set; }

    public float StoppingDistance { get; set; } = 8f;

    public Vector2F TargetOffset { get; set; } = Vector2F.Zero;

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();

        var toTarget = (_target.Position + TargetOffset) - transform.Position;
        var distance = toTarget.Length;

        if (distance <= StoppingDistance)
        {
            return;
        }

        var direction = toTarget.Normalized();
        transform.Position += direction * Speed * time.DeltaSeconds;
    }
}
