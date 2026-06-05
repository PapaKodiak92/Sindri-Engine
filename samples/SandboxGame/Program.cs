using Sindri.Behaviors2D.Components;
using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Platform.Windows;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Scenes;

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

internal sealed class SandboxScene : Scene2D
{
    private const float PlayerSize = 48f;
    private const float PlayerSpeed = 320f;

    private IInputDevice? _input;

    protected override void OnEnter(SceneContext context)
    {
        _input = context.Services.GetRequiredService<IInputDevice>();
        BackgroundColor = ColorRGBA.SindriBlue;

        var player = CreateEntity("Player");

        player.AddComponent(new Transform2D
        {
            Position = new Vector2F(
                1280f / 2f - PlayerSize / 2f,
                720f / 2f - PlayerSize / 2f)
        });

        player.AddComponent(new KeyboardMove2DComponent(_input, PlayerSpeed));
        player.AddComponent(new RectangleRenderer2D(PlayerSize, PlayerSize, ColorRGBA.SindriGold));

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("WASD / Arrow Keys move. ESC exits.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_input?.WasKeyPressed(Key.Escape) == true)
        {
            Context?.RequestExit();
        }
    }

    protected override void OnExit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }
}
