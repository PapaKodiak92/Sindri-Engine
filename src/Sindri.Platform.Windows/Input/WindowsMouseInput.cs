using Sindri.Input;
using Sindri.Platform.Windows.Native;

namespace Sindri.Platform.Windows.Input;

internal sealed class WindowsMouseInput : IMouseDevice
{
    private readonly nint _hwnd;
    private readonly HashSet<MouseButton> _currentButtons = new();
    private readonly HashSet<MouseButton> _previousButtons = new();

    public WindowsMouseInput(nint hwnd)
    {
        _hwnd = hwnd;
    }

    public MousePosition Position { get; private set; }

    public void Update()
    {
        UpdatePosition();
        UpdateButtons();
    }

    public bool IsButtonDown(MouseButton button)
    {
        return _currentButtons.Contains(button);
    }

    public bool WasButtonPressed(MouseButton button)
    {
        return _currentButtons.Contains(button) && !_previousButtons.Contains(button);
    }

    public bool WasButtonReleased(MouseButton button)
    {
        return !_currentButtons.Contains(button) && _previousButtons.Contains(button);
    }

    private void UpdatePosition()
    {
        if (!Win32.GetCursorPos(out var point))
        {
            return;
        }

        if (!Win32.ScreenToClient(_hwnd, ref point))
        {
            return;
        }

        Position = new MousePosition(point.X, point.Y);
    }

    private void UpdateButtons()
    {
        _previousButtons.Clear();

        foreach (var button in _currentButtons)
        {
            _previousButtons.Add(button);
        }

        _currentButtons.Clear();

        foreach (var button in Enum.GetValues<MouseButton>())
        {
            if (button == MouseButton.Unknown)
            {
                continue;
            }

            var virtualKey = ToVirtualKey(button);

            if (virtualKey == 0)
            {
                continue;
            }

            if ((Win32.GetAsyncKeyState(virtualKey) & 0x8000) != 0)
            {
                _currentButtons.Add(button);
            }
        }
    }

    private static int ToVirtualKey(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => 0x01,
            MouseButton.Right => 0x02,
            MouseButton.Middle => 0x04,
            _ => 0
        };
    }
}
