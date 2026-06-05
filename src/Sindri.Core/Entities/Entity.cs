namespace Sindri.Core.Entities;

public sealed class Entity
{
    private readonly List<Component> _components = new();

    public Entity(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Entity" : name;
    }

    public string Name { get; }

    public bool IsActive { get; set; } = true;

    public TComponent AddComponent<TComponent>(TComponent component)
        where TComponent : Component
    {
        ArgumentNullException.ThrowIfNull(component);

        if (component.Entity is not null)
        {
            throw new InvalidOperationException("Component is already attached to an entity.");
        }

        _components.Add(component);
        component.Attach(this);

        return component;
    }

    public TComponent? GetComponent<TComponent>()
        where TComponent : Component
    {
        foreach (var component in _components)
        {
            if (component is TComponent typedComponent)
            {
                return typedComponent;
            }
        }

        return null;
    }

    public TComponent GetRequiredComponent<TComponent>()
        where TComponent : Component
    {
        return GetComponent<TComponent>()
            ?? throw new InvalidOperationException($"Entity '{Name}' does not have required component: {typeof(TComponent).FullName}");
    }

    public void Update(SindriTime time)
    {
        if (!IsActive)
        {
            return;
        }

        foreach (var component in _components)
        {
            component.Update(time);
        }
    }
}
