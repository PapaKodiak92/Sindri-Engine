using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Input;
using Sindri.Renderer2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class CameraZoom2DComponent : Component
{
    private readonly InputActionMap _actions;

    public CameraZoom2DComponent(InputActionMap actions)
    {
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
        UpdateOrder = ComponentUpdateOrder.Input;
    }

    public string ZoomInAction { get; set; } = "ZoomIn";

    public string ZoomOutAction { get; set; } = "ZoomOut";

    public float ZoomSpeed { get; set; } = 1.5f;

    public float MinZoom { get; set; } = 0.5f;

    public float MaxZoom { get; set; } = 2.5f;

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var camera = Entity.GetRequiredComponent<Camera2D>();
        var zoom = camera.Zoom;

        if (_actions.IsDown(ZoomInAction))
        {
            zoom += ZoomSpeed * time.DeltaSeconds;
        }

        if (_actions.IsDown(ZoomOutAction))
        {
            zoom -= ZoomSpeed * time.DeltaSeconds;
        }

        camera.Zoom = System.Math.Clamp(zoom, MinZoom, MaxZoom);
    }
}
