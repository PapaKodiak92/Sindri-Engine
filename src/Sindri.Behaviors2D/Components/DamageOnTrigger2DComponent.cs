using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Physics2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class DamageOnTrigger2DComponent : Component
{
    private readonly Trigger2DComponent _trigger;

    public DamageOnTrigger2DComponent(Trigger2DComponent trigger, int damage)
    {
        _trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Damage = damage;
        _trigger.Entered += OnTriggerEntered;
    }

    public int Damage { get; set; }

    public bool DestroySelfAfterHit { get; set; } = true;

    protected override void OnDestroyed()
    {
        _trigger.Entered -= OnTriggerEntered;
    }

    public override void Update(SindriTime time)
    {
    }

    private void OnTriggerEntered(Entity target)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var health = target.GetComponent<Health2DComponent>();

        if (health is null)
        {
            return;
        }

        health.ApplyDamage(Damage);

        if (DestroySelfAfterHit)
        {
            Entity.Destroy();
        }
    }
}
