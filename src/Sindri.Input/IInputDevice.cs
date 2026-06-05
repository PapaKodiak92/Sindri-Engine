namespace Sindri.Input;

public interface IInputDevice
{
    void Update();

    bool IsKeyDown(Key key);

    bool WasKeyPressed(Key key);

    bool WasKeyReleased(Key key);
}
