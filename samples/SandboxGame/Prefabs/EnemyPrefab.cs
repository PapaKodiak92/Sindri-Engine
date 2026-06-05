using Sindri.Behaviors2D.Components;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Tilemaps;

internal sealed record EnemyPrefabConfig(
    Scene TriggerScene,
    string Name,
    float X,
    float Y,
    Transform2D Target,
    TileMap2D TileMap,
    Vector2F MapWorldPosition,
    Action? OnDied);

internal sealed class EnemyPrefab : IPrefab<EnemyPrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, EnemyPrefabConfig config)
    {
        const float enemySize = 44f;
        const float enemySpeed = 120f;

        var enemy = spawner.SpawnEntity(config.Name);
        enemy.AddTag("Enemy");
        enemy.AddTag("Damageable");

        enemy.AddComponent(new Transform2D
        {
            Position = new Vector2F(config.X, config.Y)
        });

        enemy.AddComponent(new BoxCollider2D(enemySize, enemySize));

        enemy.AddComponent(new ChaseTarget2DComponent(config.Target, enemySpeed)
        {
            TargetOffset = new Vector2F(24f, 24f),
            StoppingDistance = 48f
        });

        enemy.AddComponent(new TileMapCollision2DComponent(config.TileMap)
        {
            MapWorldPosition = config.MapWorldPosition,
            UseAxisSeparation = true,
            MaxAxisResolveDistance = enemySize * 2f
        });

        var contactTrigger = enemy.AddComponent(new Trigger2DComponent(config.TriggerScene)
        {
            TargetTag = "Player"
        });

        enemy.AddComponent(new ContactDamage2DComponent(contactTrigger, damage: 1)
        {
            CooldownSeconds = 0.75f
        });

        var health = enemy.AddComponent(new Health2DComponent(maxHealth: 4)
        {
            DestroyEntityOnDeath = true
        });

        health.Damaged += (_, amount) =>
        {
            Console.WriteLine($"{config.Name} took {amount} damage. HP {health.CurrentHealth}/{health.MaxHealth}");
        };

        health.Died += _ =>
        {
            Console.WriteLine($"{config.Name} defeated.");
            config.OnDied?.Invoke();
        };

        var bodyRenderer = enemy.AddComponent(new RectangleRenderer2D(enemySize, enemySize, new ColorRGBA(140, 70, 190))
        {
            ClampToViewport = false,
            RenderLayer = 9
        });

        enemy.AddComponent(new DamageFlash2DComponent(health, bodyRenderer)
        {
            FlashColor = ColorRGBA.White,
            FlashDurationSeconds = 0.12f
        });

        enemy.AddComponent(new HealthBar2DRenderer(health)
        {
            Width = enemySize,
            Height = 6f,
            Offset = new Vector2F(0f, -12f),
            RenderLayer = 30
        });

        return enemy;
    }
}
