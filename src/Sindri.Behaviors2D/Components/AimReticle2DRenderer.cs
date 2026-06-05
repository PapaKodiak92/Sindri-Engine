using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class AimReticle2DRenderer : RenderComponent
{
    private readonly IMouseDevice _mouse;
    private readonly Camera2D _camera;

    public AimReticle2DRenderer(IMouseDevice mouse, Camera2D camera)
    {
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));

        RenderLayer = 40_000;
        RenderOrder = 0;
        RenderSpace = RenderSpace.World;
    }

    public ColorRGBA Color { get; set; } = ColorRGBA.SindriGold;

    public float Size { get; set; } = 28f;

    public float Thickness { get; set; } = 3f;

    public bool DrawCenterDot { get; set; } = true;

    public override void Render(IGraphicsDevice graphics)
    {
        var mouseScreen = new Vector2F(_mouse.Position.X, _mouse.Position.Y);
        var world = _camera.ScreenToWorld(mouseScreen);

        var half = Size / 2f;
        var x = world.X - half;
        var y = world.Y - half;
        var thickness = System.MathF.Max(1f, Thickness);

        graphics.FillRectangle(new Rect2D(x, y, Size, thickness), Color);
        graphics.FillRectangle(new Rect2D(x, y + Size - thickness, Size, thickness), Color);
        graphics.FillRectangle(new Rect2D(x, y, thickness, Size), Color);
        graphics.FillRectangle(new Rect2D(x + Size - thickness, y, thickness, Size), Color);

        if (DrawCenterDot)
        {
            graphics.FillRectangle(
                new Rect2D(world.X - thickness / 2f, world.Y - thickness / 2f, thickness, thickness),
                Color);
        }
    }
}
