using System.Text.Json;

namespace Sindri.Input;

public sealed class InputActionMap
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

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

    public void Save(string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var actionNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var actionName in _keyBindings.Keys)
        {
            actionNames.Add(actionName);
        }

        foreach (var actionName in _mouseBindings.Keys)
        {
            actionNames.Add(actionName);
        }

        var data = new InputActionMapData();

        foreach (var actionName in actionNames)
        {
            var binding = new InputActionBindingData
            {
                Action = actionName
            };

            if (_keyBindings.TryGetValue(actionName, out var keys))
            {
                foreach (var key in keys)
                {
                    binding.Keys.Add(key.ToString());
                }
            }

            if (_mouseBindings.TryGetValue(actionName, out var buttons))
            {
                foreach (var button in buttons)
                {
                    binding.MouseButtons.Add(button.ToString());
                }
            }

            data.Actions.Add(binding);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(path, json);
    }

    public void Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Input action map file was not found.", path);
        }

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<InputActionMapData>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize input action map.");

        Clear();

        foreach (var binding in data.Actions)
        {
            if (string.IsNullOrWhiteSpace(binding.Action))
            {
                continue;
            }

            foreach (var keyName in binding.Keys)
            {
                if (Enum.TryParse<Key>(keyName, ignoreCase: true, out var key))
                {
                    BindKey(binding.Action, key);
                }
            }

            foreach (var buttonName in binding.MouseButtons)
            {
                if (Enum.TryParse<MouseButton>(buttonName, ignoreCase: true, out var button))
                {
                    BindMouseButton(binding.Action, button);
                }
            }
        }
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

    private sealed class InputActionMapData
    {
        public List<InputActionBindingData> Actions { get; set; } = new();
    }

    private sealed class InputActionBindingData
    {
        public string Action { get; set; } = string.Empty;

        public List<string> Keys { get; set; } = new();

        public List<string> MouseButtons { get; set; } = new();
    }
}
