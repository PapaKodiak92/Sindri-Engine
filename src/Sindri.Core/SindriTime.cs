namespace Sindri.Core;

public readonly record struct SindriTime(TimeSpan Delta, TimeSpan Total)
{
    public float DeltaSeconds => (float)Delta.TotalSeconds;

    public float TotalSeconds => (float)Total.TotalSeconds;
}
