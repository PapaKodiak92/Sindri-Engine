using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Scenes;

internal sealed class MainMenuScene : Scene2D
{
    private const string InputBindingsPath = "runtime-data/input/sandbox.actions.json";

    private const string StartAction = "Restart";
    private const string ExitAction = "Exit";

    private InputActionMap? _actions;

    protected override void OnEnter(SceneContext context)
    {
        _actions = context.Services.GetRequiredService<InputActionMap>();
        ConfigureMenuInputActions(_actions);

        BackgroundColor = ColorRGBA.Black;

        var title = CreateEntity("Main Menu Title");

        title.AddComponent(new Transform2D
        {
            Position = new Vector2F(500f, 260f)
        });

        title.AddComponent(new TextRenderer2D("SINDRI SANDBOX", ColorRGBA.SindriGold)
        {
            RenderSpace = RenderSpace.Screen,
            RenderLayer = 10_000
        });

        var subtitle = CreateEntity("Main Menu Subtitle");

        subtitle.AddComponent(new Transform2D
        {
            Position = new Vector2F(420f, 320f)
        });

        subtitle.AddComponent(new TextRenderer2D("Press Enter to start or Escape to exit.", ColorRGBA.White)
        {
            RenderSpace = RenderSpace.Screen,
            RenderLayer = 10_000
        });

        Console.WriteLine("Main menu. Press Enter to start or Escape to exit.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_actions?.WasPressed(StartAction) == true)
        {
            Context?.ChangeScene(new SandboxScene());
            return;
        }

        if (_actions?.WasPressed(ExitAction) == true)
        {
            Context?.RequestExit();
        }
    }

    private static void ConfigureMenuInputActions(InputActionMap actions)
    {
        if (File.Exists(InputBindingsPath))
        {
            actions.Load(InputBindingsPath);

            var changed = false;

            if (!actions.HasAction(StartAction))
            {
                actions.BindKey(StartAction, Key.Enter);
                changed = true;
            }

            if (!actions.HasAction(ExitAction))
            {
                actions.BindKey(ExitAction, Key.Escape);
                changed = true;
            }

            if (changed)
            {
                actions.Save(InputBindingsPath);
                Console.WriteLine($"Updated input bindings at {InputBindingsPath}");
            }

            return;
        }

        actions.Clear();

        actions.BindKey(StartAction, Key.Enter);
        actions.BindKey(ExitAction, Key.Escape);

        actions.Save(InputBindingsPath);
        Console.WriteLine($"Created menu input bindings at {InputBindingsPath}");
    }
}
