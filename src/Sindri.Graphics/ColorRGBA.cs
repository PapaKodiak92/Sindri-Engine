namespace Sindri.Graphics;

public readonly record struct ColorRGBA(byte R, byte G, byte B, byte A = 255)
{
    public static readonly ColorRGBA Black = new(0, 0, 0);
    public static readonly ColorRGBA White = new(255, 255, 255);
    public static readonly ColorRGBA SindriBlue = new(18, 24, 38);
}
