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
        graphics.Clear(BackgroundColor);

        OnBeforeRender(graphics);

        if (ActiveCamera is not null)
        {
            ActiveCamera.ViewportSize = graphics.ViewportSize;
            graphics.DrawOffset = ActiveCamera.GetDrawOffset();
        }

        foreach (var entity in Entities)
        {
            if (!entity.IsActive)
            {
                continue;
            }

            foreach (var renderer in entity.GetComponents<RenderComponent>())
            {
                renderer.Render(graphics);
            }
        }

        graphics.DrawOffset = Vector2F.Zero;

        OnAfterRender(graphics);
    }

    protected virtual void OnBeforeRender(IGraphicsDevice graphics)
    {
    }

    protected virtual void OnAfterRender(IGraphicsDevice graphics)
    {
    }
}
