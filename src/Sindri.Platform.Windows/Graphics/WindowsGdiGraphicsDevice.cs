using Sindri.Graphics;
using Sindri.Platform.Windows.Native;

namespace Sindri.Platform.Windows.Graphics;

internal sealed class WindowsGdiGraphicsDevice : IGraphicsDevice
{
    private readonly nint _hwnd;

    public WindowsGdiGraphicsDevice(nint hwnd)
    {
        _hwnd = hwnd;
    }

    public Size2D ViewportSize
    {
        get
        {
            if (!Win32.GetClientRect(_hwnd, out var rect))
            {
                return new Size2D(0, 0);
            }

            return new Size2D(
                Width: Math.Max(0, rect.Right - rect.Left),
                Height: Math.Max(0, rect.Bottom - rect.Top));
        }
    }

    public void Clear(ColorRGBA color)
    {
        var viewport = ViewportSize;

        FillRectangle(
            new Rect2D(0, 0, viewport.Width, viewport.Height),
            color);
    }

    public void FillRectangle(Rect2D rect, ColorRGBA color)
    {
        var hdc = Win32.GetDC(_hwnd);

        if (hdc == nint.Zero)
        {
            return;
        }

        try
        {
            var nativeRect = new Win32.RECT
            {
                Left = (int)MathF.Round(rect.X),
                Top = (int)MathF.Round(rect.Y),
                Right = (int)MathF.Round(rect.X + rect.Width),
                Bottom = (int)MathF.Round(rect.Y + rect.Height)
            };

            var brush = Win32.CreateSolidBrush(ToColorRef(color));

            if (brush == nint.Zero)
            {
                return;
            }

            try
            {
                Win32.FillRect(hdc, ref nativeRect, brush);
            }
            finally
            {
                Win32.DeleteObject(brush);
            }
        }
        finally
        {
            Win32.ReleaseDC(_hwnd, hdc);
        }
    }

    private static uint ToColorRef(ColorRGBA color)
    {
        return (uint)(color.R | color.G << 8 | color.B << 16);
    }
}
