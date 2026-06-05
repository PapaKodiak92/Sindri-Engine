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

    protected override void OnEnter(SceneContext context)
    {
        _input = context.Services.GetRequiredService<IInputDevice>();

        var player = CreateEntity("Player");

        player.AddComponent(new Transform2D
        {
            Position = new Vector2F(
                1280f / 2f - PlayerSize / 2f,
                720f / 2f - PlayerSize / 2f)
        });

        player.AddComponent(new KeyboardMove2DComponent(_input, PlayerSpeed));
        player.AddComponent(new RectangleRenderComponent(PlayerSize, PlayerSize, ColorRGBA.SindriGold));

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

    public void Render(IGraphicsDevice graphics)
    {
        graphics.Clear(ColorRGBA.SindriBlue);

        foreach (var entity in Entities)
        {
            if (!entity.IsActive)
            {
                continue;
            }

            foreach (var renderer in entity.GetComponents<RenderComponent>())
            {
                renderer.Render(graphics);
            }
        }
    }

    protected override void OnExit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }
}

internal sealed class KeyboardMove2DComponent : Component
{
    private readonly IInputDevice _input;
    private readonly float _speed;

    public KeyboardMove2DComponent(IInputDevice input, float speed)
    {
        _input = input;
        _speed = speed;
    }

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
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
            transform.Position += move.Normalized() * _speed * time.DeltaSeconds;
        }
    }
}

internal sealed class RectangleRenderComponent : RenderComponent
{
    private readonly float _width;
    private readonly float _height;
    private readonly ColorRGBA _color;

    public RectangleRenderComponent(float width, float height, ColorRGBA color)
    {
        _width = width;
        _height = height;
        _color = color;
    }

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var viewport = graphics.ViewportSize;

        var position = transform.Position;

        position = new Vector2F(
            X: Math.Clamp(position.X, 0f, Math.Max(0f, viewport.Width - _width)),
            Y: Math.Clamp(position.Y, 0f, Math.Max(0f, viewport.Height - _height)));

        transform.Position = position;

        graphics.FillRectangle(
            new Rect2D(position.X, position.Y, _width, _height),
            _color);
    }
}
