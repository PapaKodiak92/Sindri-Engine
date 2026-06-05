using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;

namespace Sindri.Physics2D.Components;

public sealed class BoxCollider2D : Component
{
    public BoxCollider2D(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public float Width { get; set; }

    public float Height { get; set; }

    public Vector2F Offset { get; set; } = Vector2F.Zero;

    public bool IsTrigger { get; set; }

    public Rect2D GetWorldBounds()
    {
        if (Entity is null)
        {
            return new Rect2D(0f, 0f, Width, Height);
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();

        return GetWorldBoundsAt(transform.Position);
    }

    public Rect2D GetWorldBoundsAt(Vector2F position)
    {
        return new Rect2D(
            position.X + Offset.X,
            position.Y + Offset.Y,
            Width,
            Height);
    }

    public bool Intersects(BoxCollider2D other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Intersects(GetWorldBounds(), other.GetWorldBounds());
    }

    public static bool Intersects(Rect2D a, Rect2D b)
    {
        return
            a.X < b.X + b.Width &&
            a.X + a.Width > b.X &&
            a.Y < b.Y + b.Height &&
            a.Y + a.Height > b.Y;
    }
}
