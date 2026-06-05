using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Graphics;
using Sindri.Renderer2D.Components;

namespace Sindri.Behaviors2D.Components;

public sealed class DamageFlash2DComponent : Component
{
    private readonly Health2DComponent _health;
    private readonly RectangleRenderer2D _renderer;
    private readonly ColorRGBA _normalColor;

    private float _remainingSeconds;

    public DamageFlash2DComponent(Health2DComponent health, RectangleRenderer2D renderer)
    {
        _health = health ?? throw new ArgumentNullException(nameof(health));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _normalColor = renderer.Color;

        _health.Damaged += OnDamaged;
    }

    public ColorRGBA FlashColor { get; set; } = ColorRGBA.White;

    public float FlashDurationSeconds { get; set; } = 0.12f;

    public override void Update(SindriTime time)
    {
        if (_remainingSeconds <= 0f)
        {
            return;
        }

        _remainingSeconds -= time.DeltaSeconds;

        if (_remainingSeconds <= 0f)
        {
            _renderer.Color = _normalColor;
            _remainingSeconds = 0f;
        }
    }

    protected override void OnDestroyed()
    {
        _health.Damaged -= OnDamaged;
        _renderer.Color = _normalColor;
    }

    private void OnDamaged(Health2DComponent health, int amount)
    {
        _renderer.Color = FlashColor;
        _remainingSeconds = FlashDurationSeconds;
    }
}
