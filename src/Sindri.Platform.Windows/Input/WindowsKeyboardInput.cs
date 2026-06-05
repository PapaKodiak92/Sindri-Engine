using Sindri.Input;
using Sindri.Platform.Windows.Native;

namespace Sindri.Platform.Windows.Input;

internal sealed class WindowsKeyboardInput : IInputDevice
{
    private readonly HashSet<Key> _currentKeys = new();
    private readonly HashSet<Key> _previousKeys = new();

    public void Update()
    {
        _previousKeys.Clear();

        foreach (var key in _currentKeys)
        {
            _previousKeys.Add(key);
        }

        _currentKeys.Clear();

        foreach (var key in Enum.GetValues<Key>())
        {
            if (key == Key.Unknown)
            {
                continue;
            }

            var virtualKey = ToVirtualKey(key);

            if (virtualKey == 0)
            {
                continue;
            }

            if (IsVirtualKeyDown(virtualKey))
            {
                _currentKeys.Add(key);
            }
        }
    }

    public bool IsKeyDown(Key key)
    {
        return _currentKeys.Contains(key);
    }

    public bool WasKeyPressed(Key key)
    {
        return _currentKeys.Contains(key) && !_previousKeys.Contains(key);
    }

    public bool WasKeyReleased(Key key)
    {
        return !_currentKeys.Contains(key) && _previousKeys.Contains(key);
    }

    private static bool IsVirtualKeyDown(int virtualKey)
    {
        return (Win32.GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    private static int ToVirtualKey(Key key)
    {
        return key switch
        {
            Key.Escape => 0x1B,
            Key.Space => 0x20,
            Key.Enter => 0x0D,
            Key.Tab => 0x09,
            Key.Backspace => 0x08,

            Key.Up => 0x26,
            Key.Down => 0x28,
            Key.Left => 0x25,
            Key.Right => 0x27,

            Key.W => 0x57,
            Key.A => 0x41,
            Key.S => 0x53,
            Key.D => 0x44,

            Key.I => 0x49,
            Key.J => 0x4A,
            Key.K => 0x4B,
            Key.L => 0x4C,

            Key.O => 0x4F,
            Key.P => 0x50,

            Key.Q => 0x51,
            Key.E => 0x45,
            Key.R => 0x52,
            Key.F => 0x46,

            Key.LeftShift => 0xA0,
            Key.LeftControl => 0xA2,
            Key.LeftAlt => 0xA4,

            _ => 0
        };
    }
}
