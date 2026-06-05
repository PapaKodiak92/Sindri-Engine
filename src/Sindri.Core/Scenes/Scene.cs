using Sindri.Core.Entities;

namespace Sindri.Core.Scenes;

public abstract class Scene : IScene
{
    private readonly List<Entity> _entities = new();

    protected SceneContext? Context { get; private set; }

    protected IReadOnlyList<Entity> Entities => _entities;

    public void Enter(SceneContext context)
    {
        Context = context;
        OnEnter(context);
    }

    public void Update(SindriTime time)
    {
        OnUpdate(time);

        foreach (var entity in _entities)
        {
            entity.Update(time);
        }

        RemoveDestroyedEntities();
    }

    public void Exit()
    {
        OnExit();

        foreach (var entity in _entities)
        {
            entity.Destroy();
        }

        _entities.Clear();
        Context = null;
    }

    public IEnumerable<Entity> GetEntities()
    {
        return _entities;
    }

    public IEnumerable<Entity> GetActiveEntities()
    {
        foreach (var entity in _entities)
        {
            if (entity.IsActive && !entity.IsDestroyed)
            {
                yield return entity;
            }
        }
    }

    public IEnumerable<Entity> FindEntitiesByTag(string tag)
    {
        foreach (var entity in GetActiveEntities())
        {
            if (entity.HasTag(tag))
            {
                yield return entity;
            }
        }
    }

    public IEnumerable<TComponent> FindComponents<TComponent>()
        where TComponent : Component
    {
        foreach (var entity in GetActiveEntities())
        {
            foreach (var component in entity.GetComponents<TComponent>())
            {
                yield return component;
            }
        }
    }

    protected Entity CreateEntity(string name)
    {
        var entity = new Entity(name);
        _entities.Add(entity);
        return entity;
    }

    protected void DestroyEntity(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.Destroy();
    }

    protected virtual void OnEnter(SceneContext context)
    {
    }

    protected virtual void OnUpdate(SindriTime time)
    {
    }

    protected virtual void OnExit()
    {
    }

    private void RemoveDestroyedEntities()
    {
        for (var i = _entities.Count - 1; i >= 0; i--)
        {
            if (_entities[i].IsDestroyed)
            {
                _entities.RemoveAt(i);
            }
        }
    }
}
