using Sindri.Core.Entities;

namespace Sindri.Core.Scenes;

public abstract class Scene : IScene, IEntitySpawner
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

        var snapshot = _entities.ToArray();

        foreach (var entity in snapshot)
        {
            if (entity.IsDestroyed)
            {
                continue;
            }

            entity.Update(time);
        }

        RemoveDestroyedEntities();
    }

    public void Exit()
    {
        OnExit();

        var snapshot = _entities.ToArray();

        foreach (var entity in snapshot)
        {
            entity.Destroy();
        }

        _entities.Clear();
        Context = null;
    }

    public Entity SpawnEntity(string name)
    {
        var entity = new Entity(name);
        _entities.Add(entity);
        return entity;
    }

    public IEnumerable<Entity> GetEntities()
    {
        return _entities.ToArray();
    }

    public IEnumerable<Entity> GetActiveEntities()
    {
        var snapshot = _entities.ToArray();

        foreach (var entity in snapshot)
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
        return SpawnEntity(name);
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
