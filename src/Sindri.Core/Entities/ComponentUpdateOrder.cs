namespace Sindri.Core.Entities;

public static class ComponentUpdateOrder
{
    public const int Early = -10_000;

    public const int Input = -1_000;

    public const int Movement = 0;

    public const int Physics = 1_000;

    public const int Triggers = 2_000;

    public const int Gameplay = 3_000;

    public const int Lifetime = 9_000;

    public const int Rendering = 10_000;
}
