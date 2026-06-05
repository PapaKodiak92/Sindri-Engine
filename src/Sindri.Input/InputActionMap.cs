namespace Sindri.Input;

public sealed class InputActionMap
{
    private readonly IInputDevice _keyboard;
    private readonly Dictionary<string, List<Key>> _keyBindings = new(StringComparer.OrdinalIgnoreCase);

    public InputActionMap(IInputDevice keyboard)
    {
        _keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
    }

    public void Clear()
    {
        _keyBindings.Clear();
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

    public bool IsDown(string actionName)
    {
        return AnyBoundKey(actionName, key => _keyboard.IsKeyDown(key));
    }

    public bool WasPressed(string actionName)
    {
        return AnyBoundKey(actionName, key => _keyboard.WasKeyPressed(key));
    }

    public bool WasReleased(string actionName)
    {
        return AnyBoundKey(actionName, key => _keyboard.WasKeyReleased(key));
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
}
