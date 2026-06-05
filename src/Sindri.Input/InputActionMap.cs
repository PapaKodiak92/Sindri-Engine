namespace Sindri.Input;

public sealed class InputActionMap
{
    private readonly IInputDevice _keyboard;
    private readonly Dictionary<string, List<Key>> _keyBindings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<MouseButton>> _mouseBindings = new(StringComparer.OrdinalIgnoreCase);

    private IMouseDevice? _mouse;

    public InputActionMap(IInputDevice keyboard)
    {
        _keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
    }

    public void SetMouseDevice(IMouseDevice mouse)
    {
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
    }

    public void Clear()
    {
        _keyBindings.Clear();
        _mouseBindings.Clear();
    }

    public void BindKey(string actionName, Key key)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentException("Action name cannot be null, empty, or whitespace.", nameof(actionName));
        }

        if (!_keyBindings.TryGetValue(actionName, out var keys))
        {
            keys = new List<Key>();
            _keyBindings[actionName] = keys;
        }

        if (!keys.Contains(key))
        {
            keys.Add(key);
        }
    }

    public void BindMouseButton(string actionName, MouseButton button)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentException("Action name cannot be null, empty, or whitespace.", nameof(actionName));
        }

        if (!_mouseBindings.TryGetValue(actionName, out var buttons))
        {
            buttons = new List<MouseButton>();
            _mouseBindings[actionName] = buttons;
        }

        if (!buttons.Contains(button))
        {
            buttons.Add(button);
        }
    }

    public bool IsDown(string actionName)
    {
        return AnyBoundKey(actionName, key => _keyboard.IsKeyDown(key)) ||
               AnyBoundMouseButton(actionName, button => _mouse?.IsButtonDown(button) == true);
    }

    public bool WasPressed(string actionName)
    {
        return AnyBoundKey(actionName, key => _keyboard.WasKeyPressed(key)) ||
               AnyBoundMouseButton(actionName, button => _mouse?.WasButtonPressed(button) == true);
    }

    public bool WasReleased(string actionName)
    {
        return AnyBoundKey(actionName, key => _keyboard.WasKeyReleased(key)) ||
               AnyBoundMouseButton(actionName, button => _mouse?.WasButtonReleased(button) == true);
    }

    private bool AnyBoundKey(string actionName, Func<Key, bool> predicate)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return false;
        }

        if (!_keyBindings.TryGetValue(actionName, out var keys))
        {
            return false;
        }

        foreach (var key in keys)
        {
            if (predicate(key))
            {
                return true;
            }
        }

        return false;
    }

    private bool AnyBoundMouseButton(string actionName, Func<MouseButton, bool> predicate)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return false;
        }

        if (!_mouseBindings.TryGetValue(actionName, out var buttons))
        {
            return false;
        }

        foreach (var button in buttons)
        {
            if (predicate(button))
            {
                return true;
            }
        }

        return false;
    }
}
