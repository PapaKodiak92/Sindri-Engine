using Sindri.Behaviors2D.Components;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;

internal sealed record TargetDummyPrefabConfig(
    string Name,
    float X,
    float Y,
    Action? OnDied);

internal sealed class TargetDummyPrefab : IPrefab<TargetDummyPrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, TargetDummyPrefabConfig config)
    {
        const float dummySize = 48f;

        var dummy = spawner.SpawnEntity(config.Name);
        dummy.AddTag("Damageable");

        dummy.AddComponent(new Transform2D
        {
            Position = new Vector2F(config.X, config.Y)
        });

        dummy.AddComponent(new BoxCollider2D(dummySize, dummySize));

        var health = dummy.AddComponent(new Health2DComponent(maxHealth: 3)
        {
            DestroyEntityOnDeath = true
        });

        health.Damaged += (_, amount) =>
        {
            Console.WriteLine($"{config.Name} took {amount} damage. HP {health.CurrentHealth}/{health.MaxHealth}");
        };

        health.Died += _ =>
        {
            Console.WriteLine($"{config.Name} destroyed.");
            config.OnDied?.Invoke();
        };

        var bodyRenderer = dummy.AddComponent(new RectangleRenderer2D(dummySize, dummySize, ColorRGBA.SindriRed)
        {
            ClampToViewport = false,
            RenderLayer = 9
        });

        dummy.AddComponent(new DamageFlash2DComponent(health, bodyRenderer)
        {
            FlashColor = ColorRGBA.White,
            FlashDurationSeconds = 0.12f
        });

        dummy.AddComponent(new HealthBar2DRenderer(health)
        {
            Width = dummySize,
            Height = 6f,
            Offset = new Vector2F(0f, -12f),
            RenderLayer = 30
        });

        return dummy;
    }
}
