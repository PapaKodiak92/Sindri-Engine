namespace Sindri.Core.Entities;

public abstract class Component
{
    public Entity? Entity { get; private set; }

    public int UpdateOrder { get; set; }

    internal void Attach(Entity entity)
    {
        Entity = entity;
        OnAttached();
    }

    internal void Destroy()
    {
        OnDestroyed();
        Entity = null;
    }

    protected virtual void OnAttached()
    {
    }

    protected virtual void OnDestroyed()
    {
    }

    public virtual void Update(SindriTime time)
    {
    }
}
