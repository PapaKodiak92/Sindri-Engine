using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Renderer2D.Components;

namespace Sindri.Renderer2D.Scenes;

public abstract class Scene2D : Scene, IRenderableScene
{
    public ColorRGBA BackgroundColor { get; set; } = ColorRGBA.SindriBlue;

    public Camera2D? ActiveCamera { get; protected set; }

    public void Render(IGraphicsDevice graphics)
    {
        graphics.DrawOffset = Vector2F.Zero;
        graphics.DrawScale = 1f;
        graphics.Clear(BackgroundColor);

        OnBeforeRender(graphics);

        if (ActiveCamera is not null)
        {
            ActiveCamera.ViewportSize = graphics.ViewportSize;
        }

        var worldOffset = ActiveCamera?.GetDrawOffset() ?? Vector2F.Zero;
        var worldScale = ActiveCamera is null
            ? 1f
            : System.MathF.Max(0.0001f, ActiveCamera.Zoom);

        var renderers = new List<RenderComponent>();

        foreach (var entity in GetActiveEntities())
        {
            foreach (var renderer in entity.GetComponents<RenderComponent>())
            {
                if (renderer.IsVisible)
                {
                    renderers.Add(renderer);
                }
            }
        }

        renderers.Sort(static (left, right) =>
        {
            var layerCompare = left.RenderLayer.CompareTo(right.RenderLayer);

            if (layerCompare != 0)
            {
                return layerCompare;
            }

            return left.RenderOrder.CompareTo(right.RenderOrder);
        });

        foreach (var renderer in renderers)
        {
            if (renderer.Entity is null || renderer.Entity.IsDestroyed || !renderer.IsVisible)
            {
                continue;
            }

            if (renderer.RenderSpace == RenderSpace.World)
            {
                graphics.DrawOffset = worldOffset;
                graphics.DrawScale = worldScale;
            }
            else
            {
                graphics.DrawOffset = Vector2F.Zero;
                graphics.DrawScale = 1f;
            }

            renderer.Render(graphics);
        }

        graphics.DrawOffset = Vector2F.Zero;
        graphics.DrawScale = 1f;

        OnAfterRender(graphics);
    }

    protected virtual void OnBeforeRender(IGraphicsDevice graphics)
    {
    }

    protected virtual void OnAfterRender(IGraphicsDevice graphics)
    {
    }
}
