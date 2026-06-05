using Sindri.Core;
using Sindri.Graphics;
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
    }

    public override IScene CreateInitialScene()
    {
        return new SandboxScene();
    }
}

internal sealed class SandboxScene : IScene, IRenderableScene
{
    public void Enter(SceneContext context)
    {
        Console.WriteLine("Sandbox scene entered.");
    }

    public void Update(SindriTime time)
    {
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
