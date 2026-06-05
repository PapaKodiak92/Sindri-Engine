using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;

namespace Sindri.Behaviors2D.Components;

public sealed class CameraFollow2DComponent : Component
{
    private readonly Transform2D _target;

    public CameraFollow2DComponent(Transform2D target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public Vector2F TargetOffset { get; set; } = Vector2F.Zero;

    public float FollowStrength { get; set; } = 1f;

    public override void Update(SindriTime time)
    {
        if (Entity is null)
        {
            return;
        }

        var cameraTransform = Entity.GetRequiredComponent<Transform2D>();
        var desiredPosition = _target.Position + TargetOffset;

        if (FollowStrength >= 1f)
        {
            cameraTransform.Position = desiredPosition;
            return;
        }

        var t = Math.Clamp(FollowStrength * time.DeltaSeconds * 60f, 0f, 1f);

        cameraTransform.Position = new Vector2F(
            cameraTransform.Position.X + (desiredPosition.X - cameraTransform.Position.X) * t,
            cameraTransform.Position.Y + (desiredPosition.Y - cameraTransform.Position.Y) * t);
    }
}
