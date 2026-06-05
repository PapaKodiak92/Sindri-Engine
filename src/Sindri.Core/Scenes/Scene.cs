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
    }

    public void Exit()
    {
        OnExit();
        _entities.Clear();
        Context = null;
    }

    protected Entity CreateEntity(string name)
    {
        var entity = new Entity(name);
        _entities.Add(entity);
        return entity;
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
}
