using Sindri.Core;
using Sindri.Core.Entities;

namespace Sindri.Behaviors2D.Components;

public sealed class DestroyAfterTimeComponent : Component
{
    private float _elapsedSeconds;

    public DestroyAfterTimeComponent(float lifetimeSeconds)
    {
        LifetimeSeconds = lifetimeSeconds; UpdateOrder = ComponentUpdateOrder.Lifetime;
    }

    public float LifetimeSeconds { get; set; }

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        _elapsedSeconds += time.DeltaSeconds;

        if (_elapsedSeconds >= LifetimeSeconds)
        {
            Entity.Destroy();
        }
    }
}
