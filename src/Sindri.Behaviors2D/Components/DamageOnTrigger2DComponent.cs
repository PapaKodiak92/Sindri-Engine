using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Physics2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class DamageOnTrigger2DComponent : Component
{
    private readonly Trigger2DComponent _trigger;
    private readonly HashSet<Entity> _damagedTargets = new();

    private int _hitCount;

    public DamageOnTrigger2DComponent(Trigger2DComponent trigger, int damage)
    {
        _trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Damage = damage;
        UpdateOrder = ComponentUpdateOrder.Gameplay;

        _trigger.Entered += OnTriggerEntered;
    }

    public int Damage { get; set; }

    public bool DestroySelfAfterHit { get; set; } = true;

    // 0 means unlimited valid targets.
    public int MaxHits { get; set; } = 1;

    public int HitCount => _hitCount;

    protected override void OnDestroyed()
    {
        _trigger.Entered -= OnTriggerEntered;
        _damagedTargets.Clear();
    }

    public override void Update(SindriTime time)
    {
    }

    private void OnTriggerEntered(Entity target)
    {
        if (Entity is null || Entity.IsDestroyed || target.IsDestroyed)
        {
            return;
        }

        if (_damagedTargets.Contains(target))
        {
            return;
        }

        if (MaxHits > 0 && _hitCount >= MaxHits)
        {
            if (DestroySelfAfterHit)
            {
                Entity.Destroy();
            }

            return;
        }

        var health = target.GetComponent<Health2DComponent>();

        if (health is null || health.IsDead)
        {
            return;
        }

        _damagedTargets.Add(target);
        _hitCount++;

        health.ApplyDamage(Damage);

        if (DestroySelfAfterHit && (MaxHits <= 0 || _hitCount >= MaxHits))
        {
            Entity.Destroy();
        }
    }
}
