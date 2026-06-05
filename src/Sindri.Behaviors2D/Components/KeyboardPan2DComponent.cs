using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Input;

namespace Sindri.Behaviors2D.Components;

public sealed class KeyboardPan2DComponent : Component
{
    private readonly IInputDevice _input;

    public KeyboardPan2DComponent(IInputDevice input, float speed)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        Speed = speed;
    }

    public float Speed { get; set; }

    public Key LeftKey { get; set; } = Key.J;

    public Key RightKey { get; set; } = Key.L;

    public Key UpKey { get; set; } = Key.I;

    public Key DownKey { get; set; } = Key.K;

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var move = Vector2F.Zero;

        if (_input.IsKeyDown(LeftKey))
        {
            move += new Vector2F(-1f, 0f);
        }

        if (_input.IsKeyDown(RightKey))
        {
            move += new Vector2F(1f, 0f);
        }

        if (_input.IsKeyDown(UpKey))
        {
            move += new Vector2F(0f, -1f);
        }

        if (_input.IsKeyDown(DownKey))
        {
            move += new Vector2F(0f, 1f);
        }

        if (move != Vector2F.Zero)
        {
            transform.Position += move.Normalized() * Speed * time.DeltaSeconds;
        }
    }
}
