using Sindri.Core.Entities;

namespace Sindri.Graphics;

public abstract class RenderComponent : Component
{
    public int RenderLayer { get; set; }

    public int RenderOrder { get; set; }

    public abstract void Render(IGraphicsDevice graphics);
}
