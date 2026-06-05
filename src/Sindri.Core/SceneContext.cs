namespace Sindri.Core;

public sealed class SceneContext
{
    internal SceneContext(EngineHost host)
    {
        Host = host;
    }

    public EngineHost Host { get; }

    public EngineServices Services => Host.Services;

    public void ChangeScene(IScene nextScene)
    {
        Host.RequestSceneChange(nextScene);
    }

    public void RequestExit()
    {
        Host.RequestExit();
    }
}
