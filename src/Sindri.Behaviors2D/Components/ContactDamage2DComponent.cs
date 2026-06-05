using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Physics2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class ContactDamage2DComponent : Component
{
    private readonly Trigger2DComponent _trigger;
    private readonly Dictionary<Entity, float> _cooldowns = new();

    public ContactDamage2DComponent(Trigger2DComponent trigger, int damage)
    {
        _trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Damage = damage;

        _trigger.Entered += TryDamage;
        _trigger.Stayed += TryDamage;
        _trigger.Exited += OnExited;
    }

    public int Damage { get; set; }

    public float CooldownSeconds { get; set; } = 0.75f;

    public override void Update(SindriTime time)
    {
        if (_cooldowns.Count == 0)
        {
            return;
        }

        var keys = _cooldowns.Keys.ToArray();

        foreach (var entity in keys)
        {
            _cooldowns[entity] -= time.DeltaSeconds;

            if (_cooldowns[entity] <= 0f || entity.IsDestroyed)
            {
                _cooldowns.Remove(entity);
            }
        }
    }

    protected override void OnDestroyed()
    {
        _trigger.Entered -= TryDamage;
        _trigger.Stayed -= TryDamage;
        _trigger.Exited -= OnExited;
        _cooldowns.Clear();
    }

    private void TryDamage(Entity target)
    {
        if (Entity is null || Entity.IsDestroyed || target.IsDestroyed)
        {
            return;
        }

        if (_cooldowns.ContainsKey(target))
        {
            return;
        }

        var health = target.GetComponent<Health2DComponent>();

        if (health is null)
        {
            return;
        }

        health.ApplyDamage(Damage);
        _cooldowns[target] = CooldownSeconds;
    }

    private void OnExited(Entity target)
    {
        _cooldowns.Remove(target);
    }
}
