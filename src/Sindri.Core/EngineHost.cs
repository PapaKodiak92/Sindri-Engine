namespace Sindri.Core;

public sealed class EngineHost
{
    private readonly SindriGame _game;
    private readonly EngineConfig _config;
    private readonly SceneContext _sceneContext;
    private IScene? _activeScene;
    private IScene? _pendingScene;
    private TimeSpan _totalTime;
    private bool _configured;
    private bool _initialized;

    public EngineHost(SindriGame game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _config = new EngineConfig();
        _sceneContext = new SceneContext(this);
        Services = new EngineServices();
    }

    public EngineConfig Config => _config;

    public EngineServices Services { get; }

    public IScene? ActiveScene => _activeScene;

    public bool ExitRequested { get; private set; }

    public void Configure()
    {
        if (_configured)
        {
            return;
        }

        _game.Configure(_config);
        _configured = true;
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        Configure();
        SwitchSceneImmediately(_game.CreateInitialScene());
        _initialized = true;
    }

    public void ChangeScene(IScene nextScene)
    {
        RequestSceneChange(nextScene);
    }

    internal void RequestSceneChange(IScene nextScene)
    {
        ArgumentNullException.ThrowIfNull(nextScene);
        _pendingScene = nextScene;
    }

    public void Tick(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "Delta time cannot be negative.");
        }

        _totalTime += delta;

        _activeScene?.Update(new SindriTime(delta, _totalTime));

        ApplyPendingSceneChange();
    }

    public void RequestExit()
    {
        ExitRequested = true;
    }

    public void Shutdown()
    {
        _pendingScene = null;

        _activeScene?.Exit();
        _activeScene = null;
        _initialized = false;
    }

    private void ApplyPendingSceneChange()
    {
        if (_pendingScene is null)
        {
            return;
        }

        var nextScene = _pendingScene;
        _pendingScene = null;

        SwitchSceneImmediately(nextScene);
    }

    private void SwitchSceneImmediately(IScene nextScene)
    {
        ArgumentNullException.ThrowIfNull(nextScene);

        _activeScene?.Exit();
        _activeScene = nextScene;
        _activeScene.Enter(_sceneContext);
    }
}
