using Sindri.Behaviors2D.Components;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;

internal sealed record MeleeHitboxPrefabConfig(
    Scene TriggerScene,
    string Name,
    float X,
    float Y,
    float Size,
    int Damage,
    ColorRGBA Color,
    float LifetimeSeconds);

internal sealed class MeleeHitboxPrefab : IPrefab<MeleeHitboxPrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, MeleeHitboxPrefabConfig config)
    {
        var size = System.MathF.Max(4f, config.Size);

        var hitbox = spawner.SpawnEntity(config.Name);
        hitbox.AddTag("MeleeHitbox");

        hitbox.AddComponent(new Transform2D
        {
            Position = new Vector2F(config.X, config.Y)
        });

        hitbox.AddComponent(new BoxCollider2D(size, size)
        {
            IsTrigger = true
        });

        var trigger = hitbox.AddComponent(new Trigger2DComponent(config.TriggerScene)
        {
            TargetTag = "Damageable"
        });

        hitbox.AddComponent(new DamageOnTrigger2DComponent(trigger, config.Damage)
        {
            DestroySelfAfterHit = false,
            MaxHits = 0,
            KnockbackDistance = 12f
        });

        hitbox.AddComponent(new DestroyAfterTimeComponent(config.LifetimeSeconds));

        hitbox.AddComponent(new RectangleRenderer2D(size, size, config.Color)
        {
            ClampToViewport = false,
            RenderLayer = 19
        });

        return hitbox;
    }
}
