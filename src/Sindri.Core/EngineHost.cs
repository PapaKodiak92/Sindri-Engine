namespace Sindri.Core;

public sealed class EngineHost
{
    private readonly SindriGame _game;
    private readonly EngineConfig _config;
    private readonly SceneContext _sceneContext;
    private IScene? _activeScene;
    private TimeSpan _totalTime;

    public EngineHost(SindriGame game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _config = new EngineConfig();
        _sceneContext = new SceneContext(this);
    }

    public EngineConfig Config => _config;

    public IScene? ActiveScene => _activeScene;

    public void Initialize()
    {
        _game.Configure(_config);
        ChangeScene(_game.CreateInitialScene());
    }

    public void ChangeScene(IScene nextScene)
    {
        ArgumentNullException.ThrowIfNull(nextScene);

        _activeScene?.Exit();
        _activeScene = nextScene;
        _activeScene.Enter(_sceneContext);
    }

    public void Tick(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "Delta time cannot be negative.");
        }

        _totalTime += delta;
        _activeScene?.Update(new SindriTime(delta, _totalTime));
    }

    public void Shutdown()
    {
        _activeScene?.Exit();
        _activeScene = null;
    }
}
