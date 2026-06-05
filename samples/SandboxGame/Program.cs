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
    private const float PlayerSize = 48f;
    private const float PlayerSpeed = 320f;

    private SceneContext? _context;
    private IInputDevice? _input;

    private float _playerX = 1280f / 2f - PlayerSize / 2f;
    private float _playerY = 720f / 2f - PlayerSize / 2f;

    public void Enter(SceneContext context)
    {
        _context = context;
        _input = context.Services.GetRequiredService<IInputDevice>();

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("WASD / Arrow Keys move. ESC exits.");
    }

    public void Update(SindriTime time)
    {
        if (_input is null)
        {
            return;
        }

        if (_input.WasKeyPressed(Key.Escape))
        {
            _context?.RequestExit();
            return;
        }

        var moveX = 0f;
        var moveY = 0f;

        if (_input.IsKeyDown(Key.A) || _input.IsKeyDown(Key.Left))
        {
            moveX -= 1f;
        }

        if (_input.IsKeyDown(Key.D) || _input.IsKeyDown(Key.Right))
        {
            moveX += 1f;
        }

        if (_input.IsKeyDown(Key.W) || _input.IsKeyDown(Key.Up))
        {
            moveY -= 1f;
        }

        if (_input.IsKeyDown(Key.S) || _input.IsKeyDown(Key.Down))
        {
            moveY += 1f;
        }

        if (moveX != 0f || moveY != 0f)
        {
            var length = MathF.Sqrt(moveX * moveX + moveY * moveY);
            moveX /= length;
            moveY /= length;

            _playerX += moveX * PlayerSpeed * time.DeltaSeconds;
            _playerY += moveY * PlayerSpeed * time.DeltaSeconds;
        }
    }

    public void Render(IGraphicsDevice graphics)
    {
        graphics.Clear(ColorRGBA.SindriBlue);

        var viewport = graphics.ViewportSize;

        _playerX = Math.Clamp(_playerX, 0f, Math.Max(0f, viewport.Width - PlayerSize));
        _playerY = Math.Clamp(_playerY, 0f, Math.Max(0f, viewport.Height - PlayerSize));

        graphics.FillRectangle(
            new Rect2D(_playerX, _playerY, PlayerSize, PlayerSize),
            ColorRGBA.SindriGold);
    }

    public void Exit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }
}
