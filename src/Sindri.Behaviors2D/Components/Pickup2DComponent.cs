using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Physics2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class Pickup2DComponent : Component
{
    private readonly Trigger2DComponent _trigger;

    public Pickup2DComponent(Trigger2DComponent trigger)
    {
        _trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        _trigger.Entered += Collect;
    }

    public bool WasCollected { get; private set; }

    public event Action<Pickup2DComponent>? Collected;

    public override void Update(SindriTime time)
    {
    }

    protected override void OnDestroyed()
    {
        _trigger.Entered -= Collect;
    }

    private void Collect(Entity collector)
    {
        if (Entity is null || WasCollected || Entity.IsDestroyed)
        {
            return;
        }

        WasCollected = true;
        Collected?.Invoke(this);
        Entity.Destroy();
    }
}
