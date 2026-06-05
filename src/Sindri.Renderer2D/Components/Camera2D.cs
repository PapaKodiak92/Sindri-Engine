using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Components;

public sealed class Camera2D : Component
{
    public float Zoom { get; set; } = 1f;

    public Size2D ViewportSize { get; internal set; }

    public Vector2F GetDrawOffset()
    {
        if (Entity is null)
        {
            return Vector2F.Zero;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();

        return new Vector2F(
            ViewportSize.Width / 2f - transform.Position.X,
            ViewportSize.Height / 2f - transform.Position.Y);
    }

    public Vector2F ScreenToWorld(Vector2F screenPosition)
    {
        return screenPosition - GetDrawOffset();
    }
}
