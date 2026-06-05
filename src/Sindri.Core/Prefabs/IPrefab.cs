using Sindri.Core.Entities;

namespace Sindri.Core.Prefabs;

public interface IPrefab<in TConfig>
{
    Entity Create(IEntitySpawner spawner, TConfig config);
}
