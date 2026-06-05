using Sindri.Core;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Platform.Windows;

return WindowsGameRunner.Run(new SandboxGame());

internal sealed class SandboxGame : SindriGame
{
    public override string Name => "Sindri Sandbox";

    public override void Configure(EngineConfig config)
    {
        config.WindowTitle = Name;
        config.TargetWidth = 1280;
        config.TargetHeight = 720;
        config.LimitFrameRate = true;
        config.TargetFramesPerSecond = 60;
        config.ShowFrameRateInTitle = true;
    }

    public override IScene CreateInitialScene()
    {
        return new SandboxScene();
    }
}

internal sealed class SandboxScene : IScene, IRenderableScene
{
    private SceneContext? _context;
    private IInputDevice? _input;

    public void Enter(SceneContext context)
    {
        _context = context;
        _input = context.Services.GetRequiredService<IInputDevice>();

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("Press ESC to exit.");
    }

    public void Update(SindriTime time)
    {
        if (_input?.WasKeyPressed(Key.Escape) == true)
        {
            _context?.RequestExit();
        }
    }

    public void Render(IGraphicsDevice graphics)
    {
        graphics.Clear(ColorRGBA.SindriBlue);
    }

    public void Exit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }
}
