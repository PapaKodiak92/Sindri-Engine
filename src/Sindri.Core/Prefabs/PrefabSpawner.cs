using Sindri.Core.Entities;

namespace Sindri.Core.Prefabs;

public sealed class PrefabSpawner
{
    private readonly IEntitySpawner _spawner;

    public PrefabSpawner(IEntitySpawner spawner)
    {
        _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
    }

    public Entity Spawn<TConfig>(IPrefab<TConfig> prefab, TConfig config)
    {
        ArgumentNullException.ThrowIfNull(prefab);
        return prefab.Create(_spawner, config);
    }
}
