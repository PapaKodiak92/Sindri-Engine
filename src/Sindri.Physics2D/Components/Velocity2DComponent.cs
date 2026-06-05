using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;

namespace Sindri.Physics2D.Components;

public sealed class Velocity2DComponent : Component
{
    public Velocity2DComponent()
    {
        UpdateOrder = ComponentUpdateOrder.Movement;
    }

    public Vector2F Velocity { get; set; } = Vector2F.Zero;

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        transform.Position += Velocity * time.DeltaSeconds;
    }
}
