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
    private const float CameraSpeed = 420f;

    private IInputDevice? _keyboard;

    protected override void OnEnter(SceneContext context)
    {
        _keyboard = context.Services.GetRequiredService<IInputDevice>();
        var mouse = context.Services.GetRequiredService<IMouseDevice>();

        BackgroundColor = ColorRGBA.SindriBlue;

        var cameraEntity = CreateEntity("Camera");
        cameraEntity.AddComponent(new Transform2D { Position = Vector2F.Zero });
        ActiveCamera = cameraEntity.AddComponent(new Camera2D());
        cameraEntity.AddComponent(new KeyboardPan2DComponent(_keyboard, CameraSpeed));

        AddWorldMarker("North West Marker", -320f, -180f, ColorRGBA.SindriRed);
        AddWorldMarker("South East Marker", 360f, 220f, ColorRGBA.SindriGreen);
        AddWorldMarker("Far Left Marker", -700f, 80f, ColorRGBA.White);
        AddWorldMarker("Far Right Marker", 700f, -80f, ColorRGBA.White);

        var player = CreateEntity("Player");

        player.AddComponent(new Transform2D
        {
            Position = Vector2F.Zero
        });

        player.AddComponent(new KeyboardMove2DComponent(_keyboard, PlayerSpeed));

        player.AddComponent(new MouseClickTeleport2DComponent(mouse)
        {
            Button = MouseButton.Left,
            Camera = ActiveCamera,
            CenterOnMouse = true,
            CenterWidth = PlayerSize,
            CenterHeight = PlayerSize
        });

        player.AddComponent(new RectangleRenderer2D(PlayerSize, PlayerSize, ColorRGBA.SindriGold)
        {
            ClampToViewport = false
        });

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("WASD / Arrow Keys move player. IJKL pans camera. Left click teleports. ESC exits.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_keyboard?.WasKeyPressed(Key.Escape) == true)
        {
            Context?.RequestExit();
        }
    }

    protected override void OnExit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }

    private Entity AddWorldMarker(string name, float x, float y, ColorRGBA color)
    {
        var marker = CreateEntity(name);

        marker.AddComponent(new Transform2D
        {
            Position = new Vector2F(x, y)
        });

        marker.AddComponent(new RectangleRenderer2D(64f, 64f, color)
        {
            ClampToViewport = false
        });

        return marker;
    }
}
