using Sindri.Core.Math;

namespace Sindri.Core.Entities;

public sealed class Transform2D : Component
{
    public Vector2F Position { get; set; } = Vector2F.Zero;

    public float RotationRadians { get; set; }

    public Vector2F Scale { get; set; } = Vector2F.One;
}
