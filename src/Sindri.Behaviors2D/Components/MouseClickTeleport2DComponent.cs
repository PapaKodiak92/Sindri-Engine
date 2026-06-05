using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Input;

namespace Sindri.Behaviors2D.Components;

public sealed class MouseClickTeleport2DComponent : Component
{
    private readonly IMouseDevice _mouse;

    public MouseClickTeleport2DComponent(IMouseDevice mouse)
    {
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
    }

    public MouseButton Button { get; set; } = MouseButton.Left;

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

        if (!_mouse.WasButtonPressed(Button))
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var mousePosition = _mouse.Position;

        var target = new Vector2F(mousePosition.X, mousePosition.Y) + OriginOffset;

        if (CenterOnMouse)
        {
            target -= new Vector2F(CenterWidth / 2f, CenterHeight / 2f);
        }

        transform.Position = target;
    }
}
