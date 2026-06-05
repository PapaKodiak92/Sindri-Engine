using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Components;

public sealed class Camera2D : Component
{
    public float Zoom { get; set; } = 1f;

    public Vector2F ScreenShakeOffset { get; set; } = Vector2F.Zero;

    public Size2D ViewportSize { get; internal set; }

    public Vector2F GetDrawOffset()
    {
        if (Entity is null)
        {
            return ScreenShakeOffset;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var zoom = System.MathF.Max(0.0001f, Zoom);

        return new Vector2F(
            ViewportSize.Width / 2f - transform.Position.X * zoom + ScreenShakeOffset.X,
            ViewportSize.Height / 2f - transform.Position.Y * zoom + ScreenShakeOffset.Y);
    }

    public Vector2F ScreenToWorld(Vector2F screenPosition)
    {
        var zoom = System.MathF.Max(0.0001f, Zoom);
        var offset = GetDrawOffset();

        return new Vector2F(
            (screenPosition.X - offset.X) / zoom,
            (screenPosition.Y - offset.Y) / zoom);
    }
}
