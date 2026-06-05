using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Platform.Windows.Native;

namespace Sindri.Platform.Windows.Graphics;

internal sealed class WindowsGdiGraphicsDevice : IGraphicsDevice
{
    private readonly nint _hwnd;

    private nint _windowHdc;
    private nint _memoryHdc;
    private nint _backBufferBitmap;
    private nint _oldBitmap;
    private bool _frameActive;
    private Size2D _frameViewportSize;

    public WindowsGdiGraphicsDevice(nint hwnd)
    {
        _hwnd = hwnd;
    }

    public Vector2F DrawOffset { get; set; } = Vector2F.Zero;

    public Size2D ViewportSize
    {
        get
        {
            if (_frameActive)
            {
                return _frameViewportSize;
            }

            if (!Win32.GetClientRect(_hwnd, out var rect))
            {
                return new Size2D(0, 0);
            }

            return new Size2D(
                Width: Math.Max(0, rect.Right - rect.Left),
                Height: Math.Max(0, rect.Bottom - rect.Top));
        }
    }

    public void BeginFrame()
    {
        if (_frameActive)
        {
            return;
        }

        _windowHdc = Win32.GetDC(_hwnd);

        if (_windowHdc == nint.Zero)
        {
            return;
        }

        if (!Win32.GetClientRect(_hwnd, out var rect))
        {
            Win32.ReleaseDC(_hwnd, _windowHdc);
            _windowHdc = nint.Zero;
            return;
        }

        _frameViewportSize = new Size2D(
            Width: Math.Max(0, rect.Right - rect.Left),
            Height: Math.Max(0, rect.Bottom - rect.Top));

        _memoryHdc = Win32.CreateCompatibleDC(_windowHdc);

        if (_memoryHdc == nint.Zero)
        {
            Win32.ReleaseDC(_hwnd, _windowHdc);
            _windowHdc = nint.Zero;
            return;
        }

        _backBufferBitmap = Win32.CreateCompatibleBitmap(
            _windowHdc,
            Math.Max(1, _frameViewportSize.Width),
            Math.Max(1, _frameViewportSize.Height));

        if (_backBufferBitmap == nint.Zero)
        {
            Win32.DeleteDC(_memoryHdc);
            Win32.ReleaseDC(_hwnd, _windowHdc);

            _memoryHdc = nint.Zero;
            _windowHdc = nint.Zero;
            return;
        }

        _oldBitmap = Win32.SelectObject(_memoryHdc, _backBufferBitmap);
        _frameActive = true;
    }

    public void EndFrame()
    {
        if (!_frameActive)
        {
            return;
        }

        try
        {
            if (_frameViewportSize.Width > 0 && _frameViewportSize.Height > 0)
            {
                Win32.BitBlt(
                    _windowHdc,
                    0,
                    0,
                    _frameViewportSize.Width,
                    _frameViewportSize.Height,
                    _memoryHdc,
                    0,
                    0,
                    Win32.SRCCOPY);
            }
        }
        finally
        {
            if (_oldBitmap != nint.Zero)
            {
                Win32.SelectObject(_memoryHdc, _oldBitmap);
            }

            if (_backBufferBitmap != nint.Zero)
            {
                Win32.DeleteObject(_backBufferBitmap);
            }

            if (_memoryHdc != nint.Zero)
            {
                Win32.DeleteDC(_memoryHdc);
            }

            if (_windowHdc != nint.Zero)
            {
                Win32.ReleaseDC(_hwnd, _windowHdc);
            }

            _windowHdc = nint.Zero;
            _memoryHdc = nint.Zero;
            _backBufferBitmap = nint.Zero;
            _oldBitmap = nint.Zero;
            _frameActive = false;
            _frameViewportSize = new Size2D(0, 0);
        }
    }

    public void Clear(ColorRGBA color)
    {
        var oldOffset = DrawOffset;
        DrawOffset = Vector2F.Zero;

        var viewport = ViewportSize;

        FillRectangle(
            new Rect2D(0, 0, viewport.Width, viewport.Height),
            color);

        DrawOffset = oldOffset;
    }

    public void FillRectangle(Rect2D rect, ColorRGBA color)
    {
        var targetHdc = _frameActive
            ? _memoryHdc
            : Win32.GetDC(_hwnd);

        if (targetHdc == nint.Zero)
        {
            return;
        }

        try
        {
            var x = rect.X + DrawOffset.X;
            var y = rect.Y + DrawOffset.Y;

            var nativeRect = new Win32.RECT
            {
                Left = (int)MathF.Round(x),
                Top = (int)MathF.Round(y),
                Right = (int)MathF.Round(x + rect.Width),
                Bottom = (int)MathF.Round(y + rect.Height)
            };

            var brush = Win32.CreateSolidBrush(ToColorRef(color));

            if (brush == nint.Zero)
            {
                return;
            }

            try
            {
                Win32.FillRect(targetHdc, ref nativeRect, brush);
            }
            finally
            {
                Win32.DeleteObject(brush);
            }
        }
        finally
        {
            if (!_frameActive)
            {
                Win32.ReleaseDC(_hwnd, targetHdc);
            }
        }
    }

    private static uint ToColorRef(ColorRGBA color)
    {
        return (uint)(color.R | color.G << 8 | color.B << 16);
    }
}
