namespace Sindri.Core;

public sealed class SceneContext
{
    internal SceneContext(EngineHost host)
    {
        Host = host;
    }

    public EngineHost Host { get; }

    public void ChangeScene(IScene nextScene)
    {
        Host.ChangeScene(nextScene);
    }
}
