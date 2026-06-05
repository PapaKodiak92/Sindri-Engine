namespace Sindri.Core.Math;

public readonly record struct Vector2F(float X, float Y)
{
    public static readonly Vector2F Zero = new(0f, 0f);
    public static readonly Vector2F One = new(1f, 1f);

    public float Length => MathF.Sqrt(X * X + Y * Y);

    public Vector2F Normalized()
    {
        var length = Length;

        if (length <= 0f)
        {
            return Zero;
        }

        return new Vector2F(X / length, Y / length);
    }

    public static Vector2F operator +(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2F operator -(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2F operator *(Vector2F value, float scalar)
    {
        return new Vector2F(value.X * scalar, value.Y * scalar);
    }

    public static Vector2F operator *(float scalar, Vector2F value)
    {
        return value * scalar;
    }
}
