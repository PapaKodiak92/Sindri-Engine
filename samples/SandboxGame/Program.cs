using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Scenes;
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

internal sealed class SandboxScene : Scene, IRenderableScene
{
    private const float PlayerSize = 48f;
    private const float PlayerSpeed = 320f;

    private IInputDevice? _input;
    private Transform2D? _playerTransform;

    protected override void OnEnter(SceneContext context)
    {
        _input = context.Services.GetRequiredService<IInputDevice>();

        var player = CreateEntity("Player");
        _playerTransform = player.AddComponent(new Transform2D
        {
            Position = new Vector2F(
                1280f / 2f - PlayerSize / 2f,
                720f / 2f - PlayerSize / 2f)
        });

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("WASD / Arrow Keys move. ESC exits.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_input is null || _playerTransform is null)
        {
            return;
        }

        if (_input.WasKeyPressed(Key.Escape))
        {
            Context?.RequestExit();
            return;
        }

        var move = Vector2F.Zero;

        if (_input.IsKeyDown(Key.A) || _input.IsKeyDown(Key.Left))
        {
            move += new Vector2F(-1f, 0f);
        }

        if (_input.IsKeyDown(Key.D) || _input.IsKeyDown(Key.Right))
        {
            move += new Vector2F(1f, 0f);
        }

        if (_input.IsKeyDown(Key.W) || _input.IsKeyDown(Key.Up))
        {
            move += new Vector2F(0f, -1f);
        }

        if (_input.IsKeyDown(Key.S) || _input.IsKeyDown(Key.Down))
        {
            move += new Vector2F(0f, 1f);
        }

        if (move != Vector2F.Zero)
        {
            _playerTransform.Position += move.Normalized() * PlayerSpeed * time.DeltaSeconds;
        }
    }

    public void Render(IGraphicsDevice graphics)
    {
        graphics.Clear(ColorRGBA.SindriBlue);

        if (_playerTransform is null)
        {
            return;
        }

        var viewport = graphics.ViewportSize;
        var position = _playerTransform.Position;

        position = new Vector2F(
            X: Math.Clamp(position.X, 0f, Math.Max(0f, viewport.Width - PlayerSize)),
            Y: Math.Clamp(position.Y, 0f, Math.Max(0f, viewport.Height - PlayerSize)));

        _playerTransform.Position = position;

        graphics.FillRectangle(
            new Rect2D(position.X, position.Y, PlayerSize, PlayerSize),
            ColorRGBA.SindriGold);
    }

    protected override void OnExit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }
}
