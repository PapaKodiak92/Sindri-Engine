namespace Sindri.Core;

public abstract class SindriGame
{
    public abstract string Name { get; }

    public virtual void Configure(EngineConfig config)
    {
    }

    public abstract IScene CreateInitialScene();
}
