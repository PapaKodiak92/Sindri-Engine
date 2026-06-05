namespace Sindri.Graphics;

public interface IGraphicsDevice
{
    Size2D ViewportSize { get; }

    void Clear(ColorRGBA color);

    void FillRectangle(Rect2D rect, ColorRGBA color);
}
