namespace Sindri.Core;

public sealed class CooldownTimer
{
    private float _remainingSeconds;

    public CooldownTimer(float durationSeconds)
    {
        DurationSeconds = durationSeconds;
    }

    public float DurationSeconds { get; set; }

    public bool IsReady => _remainingSeconds <= 0f;

    public void Update(SindriTime time)
    {
        if (_remainingSeconds > 0f)
        {
            _remainingSeconds -= time.DeltaSeconds;
        }
    }

    public bool TryUse()
    {
        if (!IsReady)
        {
            return false;
        }

        _remainingSeconds = System.Math.Max(0f, DurationSeconds);
        return true;
    }

    public void Reset()
    {
        _remainingSeconds = 0f;
    }
}
