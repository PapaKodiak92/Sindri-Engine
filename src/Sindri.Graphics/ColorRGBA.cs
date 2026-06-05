namespace Sindri.Graphics;

public readonly record struct ColorRGBA(byte R, byte G, byte B, byte A = 255)
{
    public static readonly ColorRGBA Black = new(0, 0, 0);
    public static readonly ColorRGBA White = new(255, 255, 255);
    public static readonly ColorRGBA SindriBlue = new(18, 24, 38);
    public static readonly ColorRGBA SindriGold = new(214, 164, 74);
    public static readonly ColorRGBA SindriRed = new(180, 52, 52);
    public static readonly ColorRGBA SindriGreen = new(62, 166, 96);
    public static readonly ColorRGBA SindriCyan = new(70, 160, 190);
}
