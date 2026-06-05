using Sindri.Core.Entities;

namespace Sindri.Graphics;

public abstract class RenderComponent : Component
{
    public abstract void Render(IGraphicsDevice graphics);
}
