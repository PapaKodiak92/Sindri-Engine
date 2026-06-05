using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Renderer2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class CameraShake2DComponent : Component
{
    private float _remainingSeconds;
    private float _durationSeconds;
    private float _strengthPixels;

    public CameraShake2DComponent()
    {
        UpdateOrder = ComponentUpdateOrder.Lifetime;
    }

    public void Shake(float durationSeconds, float strengthPixels)
    {
        if (durationSeconds <= 0f || strengthPixels <= 0f)
        {
            return;
        }

        _durationSeconds = System.MathF.Max(_durationSeconds, durationSeconds);
        _remainingSeconds = System.MathF.Max(_remainingSeconds, durationSeconds);
        _strengthPixels = System.MathF.Max(_strengthPixels, strengthPixels);
    }

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        var camera = Entity.GetRequiredComponent<Camera2D>();

        if (_remainingSeconds <= 0f)
        {
            camera.ScreenShakeOffset = Vector2F.Zero;
            _durationSeconds = 0f;
            _strengthPixels = 0f;
            return;
        }

        _remainingSeconds -= time.DeltaSeconds;

        var duration = System.MathF.Max(0.0001f, _durationSeconds);
        var percent = System.Math.Clamp(_remainingSeconds / duration, 0f, 1f);
        var strength = _strengthPixels * percent;

        var x = ((float)Random.Shared.NextDouble() * 2f - 1f) * strength;
        var y = ((float)Random.Shared.NextDouble() * 2f - 1f) * strength;

        camera.ScreenShakeOffset = new Vector2F(x, y);

        if (_remainingSeconds <= 0f)
        {
            camera.ScreenShakeOffset = Vector2F.Zero;
            _durationSeconds = 0f;
            _strengthPixels = 0f;
        }
    }

    protected override void OnDestroyed()
    {
        if (Entity?.GetComponent<Camera2D>() is { } camera)
        {
            camera.ScreenShakeOffset = Vector2F.Zero;
        }
    }
}
