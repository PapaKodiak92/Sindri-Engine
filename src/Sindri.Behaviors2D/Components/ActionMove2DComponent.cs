using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Input;

namespace Sindri.Behaviors2D.Components;

public sealed class ActionMove2DComponent : Component
{
    private readonly InputActionMap _actions;

    public ActionMove2DComponent(InputActionMap actions, float speed)
    {
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
        Speed = speed; UpdateOrder = ComponentUpdateOrder.Movement;
    }

    public float Speed { get; set; }

    public string MoveLeftAction { get; set; } = "MoveLeft";

    public string MoveRightAction { get; set; } = "MoveRight";

    public string MoveUpAction { get; set; } = "MoveUp";

    public string MoveDownAction { get; set; } = "MoveDown";

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();
        var move = Vector2F.Zero;

        if (_actions.IsDown(MoveLeftAction))
        {
            move += new Vector2F(-1f, 0f);
        }

        if (_actions.IsDown(MoveRightAction))
        {
            move += new Vector2F(1f, 0f);
        }

        if (_actions.IsDown(MoveUpAction))
        {
            move += new Vector2F(0f, -1f);
        }

        if (_actions.IsDown(MoveDownAction))
        {
            move += new Vector2F(0f, 1f);
        }

        if (move != Vector2F.Zero)
        {
            transform.Position += move.Normalized() * Speed * time.DeltaSeconds;
        }
    }
}
