using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Input;

namespace Sindri.Behaviors2D.Components;

public sealed class KeyboardMove2DComponent : Component
{
    private readonly IInputDevice _input;

    public KeyboardMove2DComponent(IInputDevice input, float speed)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        Speed = speed;
    }

    public float Speed { get; set; }

    public bool UseArrowKeys { get; set; } = true;

    public bool UseWasd { get; set; } = true;

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var move = Vector2F.Zero;

        if (UseWasd)
        {
            if (_input.IsKeyDown(Key.A))
            {
                move += new Vector2F(-1f, 0f);
            }

            if (_input.IsKeyDown(Key.D))
            {
                move += new Vector2F(1f, 0f);
            }

            if (_input.IsKeyDown(Key.W))
            {
                move += new Vector2F(0f, -1f);
            }

            if (_input.IsKeyDown(Key.S))
            {
                move += new Vector2F(0f, 1f);
            }
        }

        if (UseArrowKeys)
        {
            if (_input.IsKeyDown(Key.Left))
            {
                move += new Vector2F(-1f, 0f);
            }

            if (_input.IsKeyDown(Key.Right))
            {
                move += new Vector2F(1f, 0f);
            }

            if (_input.IsKeyDown(Key.Up))
            {
                move += new Vector2F(0f, -1f);
            }

            if (_input.IsKeyDown(Key.Down))
            {
                move += new Vector2F(0f, 1f);
            }
        }

        if (move != Vector2F.Zero)
        {
            transform.Position += move.Normalized() * Speed * time.DeltaSeconds;
        }
    }
}
