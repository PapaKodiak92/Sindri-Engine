namespace Sindri.Input;

public interface IMouseDevice
{
    MousePosition Position { get; }

    void Update();

    bool IsButtonDown(MouseButton button);

    bool WasButtonPressed(MouseButton button);

    bool WasButtonReleased(MouseButton button);
}
