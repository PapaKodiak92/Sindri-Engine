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

    public float DrawScale { get; set; } = 1f;

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
                Width: System.Math.Max(0, rect.Right - rect.Left),
                Height: System.Math.Max(0, rect.Bottom - rect.Top));
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
            Width: System.Math.Max(0, rect.Right - rect.Left),
            Height: System.Math.Max(0, rect.Bottom - rect.Top));

        _memoryHdc = Win32.CreateCompatibleDC(_windowHdc);

        if (_memoryHdc == nint.Zero)
        {
            Win32.ReleaseDC(_hwnd, _windowHdc);
            _windowHdc = nint.Zero;
            return;
        }

        _backBufferBitmap = Win32.CreateCompatibleBitmap(
            _windowHdc,
            System.Math.Max(1, _frameViewportSize.Width),
            System.Math.Max(1, _frameViewportSize.Height));

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
        var oldScale = DrawScale;

        DrawOffset = Vector2F.Zero;
        DrawScale = 1f;

        var viewport = ViewportSize;

        FillRectangle(
            new Rect2D(0, 0, viewport.Width, viewport.Height),
            color);

        DrawOffset = oldOffset;
        DrawScale = oldScale;
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
            var scale = System.MathF.Max(0.0001f, DrawScale);

            var x = rect.X * scale + DrawOffset.X;
            var y = rect.Y * scale + DrawOffset.Y;
            var width = rect.Width * scale;
            var height = rect.Height * scale;

            var nativeRect = new Win32.RECT
            {
                Left = (int)System.MathF.Round(x),
                Top = (int)System.MathF.Round(y),
                Right = (int)System.MathF.Round(x + width),
                Bottom = (int)System.MathF.Round(y + height)
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

    public void DrawText(string text, Vector2F position, ColorRGBA color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var targetHdc = _frameActive
            ? _memoryHdc
            : Win32.GetDC(_hwnd);

        if (targetHdc == nint.Zero)
        {
            return;
        }

        try
        {
            var scale = System.MathF.Max(0.0001f, DrawScale);

            var x = position.X * scale + DrawOffset.X;
            var y = position.Y * scale + DrawOffset.Y;

            Win32.SetBkMode(targetHdc, Win32.TRANSPARENT);
            Win32.SetTextColor(targetHdc, ToColorRef(color));
            Win32.TextOutW(
                targetHdc,
                (int)System.MathF.Round(x),
                (int)System.MathF.Round(y),
                text,
                text.Length);
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
