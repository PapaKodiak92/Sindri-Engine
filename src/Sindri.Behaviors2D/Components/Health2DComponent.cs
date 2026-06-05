using Sindri.Core;
using Sindri.Core.Entities;

namespace Sindri.Behaviors2D.Components;

public sealed class Health2DComponent : Component
{
    public Health2DComponent(int maxHealth)
    {
        if (maxHealth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHealth));
        }

        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public int MaxHealth { get; }

    public int CurrentHealth { get; private set; }

    public bool DestroyEntityOnDeath { get; set; } = true;

    public bool IsDead => CurrentHealth <= 0;

    public event Action<Health2DComponent, int>? Damaged;

    public event Action<Health2DComponent>? Died;

    public void ApplyDamage(int amount)
    {
        if (Entity is null || Entity.IsDestroyed || amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        Damaged?.Invoke(this, amount);

        if (CurrentHealth <= 0)
        {
            Died?.Invoke(this);

            if (DestroyEntityOnDeath)
            {
                Entity.Destroy();
            }
        }
    }

    public override void Update(SindriTime time)
    {
    }
}
