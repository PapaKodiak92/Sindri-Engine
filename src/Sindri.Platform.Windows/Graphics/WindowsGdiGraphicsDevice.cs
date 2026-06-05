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

    public void Clear(ColorRGBA color)
    {
        var hdc = Win32.GetDC(_hwnd);

        if (hdc == nint.Zero)
        {
            return;
        }

        try
        {
            if (!Win32.GetClientRect(_hwnd, out var rect))
            {
                return;
            }

            var brush = Win32.CreateSolidBrush(ToColorRef(color));

            if (brush == nint.Zero)
            {
                return;
            }

            try
            {
                Win32.FillRect(hdc, ref rect, brush);
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
