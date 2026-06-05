using System.ComponentModel;
using System.Runtime.InteropServices;

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

    public static NativeWindow Create(string title, int width, int height)
    {
        RegisterWindowClass();

        var hwnd = Win32.CreateWindowExW(
            0,
            WindowClassName,
            title,
            Win32.WS_OVERLAPPEDWINDOW,
            Win32.CW_USEDEFAULT,
            Win32.CW_USEDEFAULT,
            width,
            height,
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
