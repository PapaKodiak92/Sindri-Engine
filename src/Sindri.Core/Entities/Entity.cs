namespace Sindri.Core.Entities;

public sealed class Entity
{
    private readonly List<Component> _components = new();
    private readonly HashSet<string> _tags = new(StringComparer.OrdinalIgnoreCase);

    public Entity(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Entity" : name;
    }

    public string Name { get; }

    public bool IsActive { get; set; } = true;

    public bool IsDestroyed { get; private set; }

    public IReadOnlyList<Component> Components => _components;

    public IReadOnlyCollection<string> Tags => _tags;

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag cannot be null, empty, or whitespace.", nameof(tag));
        }

        _tags.Add(tag);
    }

    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        _tags.Remove(tag);
    }

    public bool HasTag(string tag)
    {
        return !string.IsNullOrWhiteSpace(tag) && _tags.Contains(tag);
    }

    public TComponent AddComponent<TComponent>(TComponent component)
        where TComponent : Component
    {
        ArgumentNullException.ThrowIfNull(component);

        if (IsDestroyed)
        {
            throw new InvalidOperationException($"Cannot add component to destroyed entity '{Name}'.");
        }

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

    public IEnumerable<TComponent> GetComponents<TComponent>()
        where TComponent : Component
    {
        foreach (var component in _components)
        {
            if (component is TComponent typedComponent)
            {
                yield return typedComponent;
            }
        }
    }

    public TComponent GetRequiredComponent<TComponent>()
        where TComponent : Component
    {
        return GetComponent<TComponent>()
            ?? throw new InvalidOperationException($"Entity '{Name}' does not have required component: {typeof(TComponent).FullName}");
    }

    public void Update(SindriTime time)
    {
        if (!IsActive || IsDestroyed)
        {
            return;
        }

        var snapshot = _components
            .OrderBy(component => component.UpdateOrder)
            .ToArray();

        foreach (var component in snapshot)
        {
            if (IsDestroyed)
            {
                break;
            }

            if (component.Entity != this)
            {
                continue;
            }

            component.Update(time);
        }
    }

    public void Destroy()
    {
        if (IsDestroyed)
        {
            return;
        }

        IsDestroyed = true;
        IsActive = false;

        var snapshot = _components.ToArray();

        foreach (var component in snapshot)
        {
            component.Destroy();
        }

        _components.Clear();
        _tags.Clear();
    }
}
