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

    public bool IsFullHealth => CurrentHealth >= MaxHealth;

    public event Action<Health2DComponent, int>? Damaged;

    public event Action<Health2DComponent, int>? Healed;

    public event Action<Health2DComponent>? Died;

    public void ApplyDamage(int amount)
    {
        if (Entity is null || Entity.IsDestroyed || amount <= 0 || IsDead)
        {
            return;
        }

        var previousHealth = CurrentHealth;
        CurrentHealth = System.Math.Max(0, CurrentHealth - amount);

        var actualDamage = previousHealth - CurrentHealth;

        if (actualDamage > 0)
        {
            Damaged?.Invoke(this, actualDamage);
        }

        if (CurrentHealth <= 0)
        {
            Died?.Invoke(this);

            if (DestroyEntityOnDeath)
            {
                Entity.Destroy();
            }
        }
    }

    public int Heal(int amount)
    {
        if (Entity is null || Entity.IsDestroyed || amount <= 0 || IsDead)
        {
            return 0;
        }

        var previousHealth = CurrentHealth;
        CurrentHealth = System.Math.Min(MaxHealth, CurrentHealth + amount);

        var actualHeal = CurrentHealth - previousHealth;

        if (actualHeal > 0)
        {
            Healed?.Invoke(this, actualHeal);
        }

        return actualHeal;
    }

    public override void Update(SindriTime time)
    {
    }
}
