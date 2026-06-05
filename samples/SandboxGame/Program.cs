using Sindri.Behaviors2D.Components;
using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Platform.Windows;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Scenes;
using Sindri.Renderer2D.Tilemaps;

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

        BackgroundColor = ColorRGBA.Black;

        var cameraEntity = CreateEntity("Camera");
        cameraEntity.AddComponent(new Transform2D { Position = Vector2F.Zero });
        ActiveCamera = cameraEntity.AddComponent(new Camera2D());
        cameraEntity.AddComponent(new KeyboardPan2DComponent(_keyboard, CameraSpeed));

        CreateTileMap();

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

    private void CreateTileMap()
    {
        const int tileSize = 64;
        const int width = 30;
        const int height = 20;

        var map = new TileMap2D(width, height, tileSize);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var checker = (x + y) % 2 == 0;

                map.SetTile(
                    x,
                    y,
                    checker
                        ? new ColorRGBA(32, 45, 58)
                        : new ColorRGBA(24, 34, 46));
            }
        }

        for (var x = 0; x < width; x++)
        {
            map.SetTile(x, 0, new ColorRGBA(80, 80, 88));
            map.SetTile(x, height - 1, new ColorRGBA(80, 80, 88));
        }

        for (var y = 0; y < height; y++)
        {
            map.SetTile(0, y, new ColorRGBA(80, 80, 88));
            map.SetTile(width - 1, y, new ColorRGBA(80, 80, 88));
        }

        map.SetTile(5, 5, ColorRGBA.SindriRed);
        map.SetTile(8, 8, ColorRGBA.SindriGreen);
        map.SetTile(12, 6, ColorRGBA.White);

        var mapEntity = CreateEntity("Test Tilemap");

        mapEntity.AddComponent(new Transform2D
        {
            Position = new Vector2F(
                -width * tileSize / 2f,
                -height * tileSize / 2f)
        });

        mapEntity.AddComponent(new TileMapRenderer2D(map));
    }
}
