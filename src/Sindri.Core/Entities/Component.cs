namespace Sindri.Core.Entities;

public abstract class Component
{
    public Entity? Entity { get; private set; }

    internal void Attach(Entity entity)
    {
        Entity = entity;
        OnAttached();
    }

    protected virtual void OnAttached()
    {
    }

    public virtual void Update(SindriTime time)
    {
    }
}
