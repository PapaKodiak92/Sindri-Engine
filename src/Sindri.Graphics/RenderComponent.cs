using Sindri.Core.Entities;

namespace Sindri.Graphics;

public abstract class RenderComponent : Component
{
    public int RenderLayer { get; set; }

    public int RenderOrder { get; set; }

    public RenderSpace RenderSpace { get; set; } = RenderSpace.World;

    public abstract void Render(IGraphicsDevice graphics);
}
