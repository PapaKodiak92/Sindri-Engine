using System.ComponentModel;
using System.Runtime.InteropServices;
using Sindri.Graphics;

namespace Sindri.Platform.Windows.Native;

internal sealed class NativeWindow : IDisposable
{
    private const string WindowClassName = "SindriEngineWindowClass";

    private static readonly Win32.WndProc WndProcDelegate = WindowProcedure;
    private static bool _classRegistered;

    private readonly nint _hwnd;
    private bool _disposed;

    private NativeWindow(nint hwnd)
    {
        _hwnd = hwnd;
    }

    public nint Handle => _hwnd;

    public static NativeWindow Create(string title, int width, int height)
    {
        RegisterWindowClass();

        var style = Win32.WS_OVERLAPPEDWINDOW;
        const uint extendedStyle = 0;

        var windowRect = new Win32.RECT
        {
            Left = 0,
            Top = 0,
            Right = width,
            Bottom = height
        };

        if (!Win32.AdjustWindowRectEx(ref windowRect, style, false, extendedStyle))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to calculate Sindri native window size.");
        }

        var windowWidth = windowRect.Right - windowRect.Left;
        var windowHeight = windowRect.Bottom - windowRect.Top;

        var hwnd = Win32.CreateWindowExW(
            extendedStyle,
            WindowClassName,
            title,
            style,
            Win32.CW_USEDEFAULT,
            Win32.CW_USEDEFAULT,
            windowWidth,
            windowHeight,
            nint.Zero,
            nint.Zero,
            Win32.GetModuleHandleW(null),
            nint.Zero);

        if (hwnd == nint.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create Sindri native window.");
        }

        return new NativeWindow(hwnd);
    }

    public void Show()
    {
        Win32.ShowWindow(_hwnd, Win32.SW_SHOW);
        Win32.UpdateWindow(_hwnd);
    }

    public void SetTitle(string title)
    {
        Win32.SetWindowTextW(_hwnd, title);
    }

    public Size2D GetClientSize()
    {
        if (!Win32.GetClientRect(_hwnd, out var rect))
        {
            return new Size2D(0, 0);
        }

        return new Size2D(
            Width: Math.Max(0, rect.Right - rect.Left),
            Height: Math.Max(0, rect.Bottom - rect.Top));
    }

    public bool ProcessMessages()
    {
        while (Win32.PeekMessageW(out var message, nint.Zero, 0, 0, Win32.PM_REMOVE))
        {
            if (message.Message == Win32.WM_QUIT)
            {
                return false;
            }

            Win32.TranslateMessage(ref message);
            Win32.DispatchMessageW(ref message);
        }

        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_hwnd != nint.Zero)
        {
            Win32.DestroyWindow(_hwnd);
        }
    }

    private static void RegisterWindowClass()
    {
        if (_classRegistered)
        {
            return;
        }

        var instance = Win32.GetModuleHandleW(null);

        var windowClass = new Win32.WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<Win32.WNDCLASSEXW>(),
            style = Win32.CS_HREDRAW | Win32.CS_VREDRAW,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(WndProcDelegate),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = instance,
            hIcon = nint.Zero,
            hCursor = Win32.LoadCursorW(nint.Zero, Win32.IDC_ARROW),
            hbrBackground = nint.Zero,
            lpszMenuName = null,
            lpszClassName = WindowClassName,
            hIconSm = nint.Zero
        };

        var atom = Win32.RegisterClassExW(ref windowClass);

        if (atom == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register Sindri native window class.");
        }

        _classRegistered = true;
    }

    private static nint WindowProcedure(nint hwnd, uint message, nuint wParam, nint lParam)
    {
        switch (message)
        {
            case Win32.WM_ERASEBKGND:
            return new nint(1);
            
            case Win32.WM_CLOSE:
                Win32.DestroyWindow(hwnd);
                return nint.Zero;

            case Win32.WM_DESTROY:
                Win32.PostQuitMessage(0);
                return nint.Zero;

            default:
                return Win32.DefWindowProcW(hwnd, message, wParam, lParam);
        }
    }
}
