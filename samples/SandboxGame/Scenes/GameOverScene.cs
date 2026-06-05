using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Scenes;

internal sealed class GameOverScene : Scene2D
{
    private const string RestartAction = "Restart";
    private const string ExitAction = "Exit";

    private InputActionMap? _actions;

    protected override void OnEnter(SceneContext context)
    {
        _actions = context.Services.GetRequiredService<InputActionMap>();

        BackgroundColor = ColorRGBA.Black;

        var title = CreateEntity("Game Over Title");

        title.AddComponent(new Transform2D
        {
            Position = new Vector2F(520f, 300f)
        });

        title.AddComponent(new TextRenderer2D("GAME OVER", ColorRGBA.SindriRed)
        {
            RenderSpace = RenderSpace.Screen,
            RenderLayer = 10_000
        });

        var instructions = CreateEntity("Game Over Instructions");

        instructions.AddComponent(new Transform2D
        {
            Position = new Vector2F(450f, 340f)
        });

        instructions.AddComponent(new TextRenderer2D("Press Enter to restart or Escape to exit.", ColorRGBA.White)
        {
            RenderSpace = RenderSpace.Screen,
            RenderLayer = 10_000
        });

        Console.WriteLine("Game over. Press Enter to restart or Escape to exit.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_actions?.WasPressed(RestartAction) == true)
        {
            Context?.ChangeScene(new SandboxScene());
            return;
        }

        if (_actions?.WasPressed(ExitAction) == true)
        {
            Context?.RequestExit();
        }
    }
}
