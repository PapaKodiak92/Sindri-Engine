namespace Sindri.Graphics;

public interface IGraphicsDevice
{
    Size2D ViewportSize { get; }

    void Clear(ColorRGBA color);
}
