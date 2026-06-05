using Sindri.Core.Entities;
using Sindri.Core.Scenes;
using Sindri.Graphics;

namespace Sindri.Renderer2D.Scenes;

public abstract class Scene2D : Scene, IRenderableScene
{
    public ColorRGBA BackgroundColor { get; set; } = ColorRGBA.SindriBlue;

    public void Render(IGraphicsDevice graphics)
    {
        graphics.Clear(BackgroundColor);

        OnBeforeRender(graphics);

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

        OnAfterRender(graphics);
    }

    protected virtual void OnBeforeRender(IGraphicsDevice graphics)
    {
    }

    protected virtual void OnAfterRender(IGraphicsDevice graphics)
    {
    }
}
