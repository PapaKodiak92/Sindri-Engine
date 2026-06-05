using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Renderer2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class CameraBounds2DComponent : Component
{
    public CameraBounds2DComponent(Rect2D worldBounds)
    {
        WorldBounds = worldBounds;
    }

    public Rect2D WorldBounds { get; set; }

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var camera = Entity.GetRequiredComponent<Camera2D>();
        var viewport = camera.ViewportSize;

        if (viewport.Width <= 0 || viewport.Height <= 0)
        {
            return;
        }

        var halfViewportWidth = viewport.Width / 2f;
        var halfViewportHeight = viewport.Height / 2f;

        var minX = WorldBounds.X + halfViewportWidth;
        var maxX = WorldBounds.X + WorldBounds.Width - halfViewportWidth;

        var minY = WorldBounds.Y + halfViewportHeight;
        var maxY = WorldBounds.Y + WorldBounds.Height - halfViewportHeight;

        var targetX = transform.Position.X;
        var targetY = transform.Position.Y;

        if (minX > maxX)
        {
            targetX = WorldBounds.X + WorldBounds.Width / 2f;
        }
        else
        {
            targetX = Math.Clamp(targetX, minX, maxX);
        }

        if (minY > maxY)
        {
            targetY = WorldBounds.Y + WorldBounds.Height / 2f;
        }
        else
        {
            targetY = Math.Clamp(targetY, minY, maxY);
        }

        transform.Position = new Vector2F(targetX, targetY);
    }
}
