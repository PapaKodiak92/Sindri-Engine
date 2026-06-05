namespace Sindri.Core;

public sealed class EngineConfig
{
    public string WindowTitle { get; set; } = "Sindri Game";

    public int TargetWidth { get; set; } = 1280;

    public int TargetHeight { get; set; } = 720;

    public bool IsFixedTimeStep { get; set; } = true;
}
