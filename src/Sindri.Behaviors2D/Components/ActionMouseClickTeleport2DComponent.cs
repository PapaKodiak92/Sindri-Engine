using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Input;
using Sindri.Renderer2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class ActionMouseClickTeleport2DComponent : Component
{
    private readonly InputActionMap _actions;
    private readonly IMouseDevice _mouse;

    public ActionMouseClickTeleport2DComponent(InputActionMap actions, IMouseDevice mouse)
    {
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
    }

    public string ActionName { get; set; } = "TeleportPlayer";

    public Camera2D? Camera { get; set; }

    public Vector2F OriginOffset { get; set; } = Vector2F.Zero;

    public bool CenterOnMouse { get; set; } = true;

    public float CenterWidth { get; set; }

    public float CenterHeight { get; set; }

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        if (!_actions.WasPressed(ActionName))
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();

        var screenPosition = new Vector2F(_mouse.Position.X, _mouse.Position.Y);
        var target = Camera?.ScreenToWorld(screenPosition) ?? screenPosition;

        target += OriginOffset;

        if (CenterOnMouse)
        {
            target -= new Vector2F(CenterWidth / 2f, CenterHeight / 2f);
        }

        transform.Position = target;
    }
}
