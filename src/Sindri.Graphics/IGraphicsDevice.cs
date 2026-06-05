using Sindri.Core.Math;

namespace Sindri.Graphics;

public interface IGraphicsDevice
{
    Size2D ViewportSize { get; }

    Vector2F DrawOffset { get; set; }

    void Clear(ColorRGBA color);

    void FillRectangle(Rect2D rect, ColorRGBA color);
}
