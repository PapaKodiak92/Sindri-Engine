using Sindri.Core;
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

internal sealed class SandboxScene : IScene
{
    public void Enter(SceneContext context)
    {
        Console.WriteLine("Sandbox scene entered.");
    }

    public void Update(SindriTime time)
    {
        // First native window milestone.
        // Rendering/input come next.
    }

    public void Exit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }
}
