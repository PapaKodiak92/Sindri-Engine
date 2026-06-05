using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Physics2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class Pickup2DComponent : Component
{
    private readonly BoxCollider2D _collector;

    public Pickup2DComponent(BoxCollider2D collector)
    {
        _collector = collector ?? throw new ArgumentNullException(nameof(collector));
    }

    public bool WasCollected { get; private set; }

    public event Action<Pickup2DComponent>? Collected;

    public override void Update(SindriTime time)
    {
        if (Entity is null || WasCollected || !Entity.IsActive)
        {
            return;
        }

        var pickupCollider = Entity.GetRequiredComponent<BoxCollider2D>();

        if (!pickupCollider.Intersects(_collector))
        {
            return;
        }

        WasCollected = true;
        Entity.IsActive = false;
        Collected?.Invoke(this);
    }
}
