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
    int MaxHealth,
    float MoveSpeed,
    int ContactDamage,
    float ContactCooldownSeconds,
    Action<int, Vector2F>? OnDamaged,
    Action? OnDied);

internal sealed class EnemyPrefab : IPrefab<EnemyPrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, EnemyPrefabConfig config)
    {
        const float enemySize = 44f;

        var enemySpeed = config.MoveSpeed <= 0f
            ? 120f
            : config.MoveSpeed;

        var contactDamage = System.Math.Max(1, config.ContactDamage);

        var contactCooldownSeconds = config.ContactCooldownSeconds <= 0f
            ? 0.75f
            : config.ContactCooldownSeconds;

        var enemy = spawner.SpawnEntity(config.Name);
        enemy.AddTag("Enemy");
        enemy.AddTag("Damageable");

        var transform = enemy.AddComponent(new Transform2D
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

        enemy.AddComponent(new ContactDamage2DComponent(contactTrigger, damage: contactDamage)
        {
            CooldownSeconds = contactCooldownSeconds
        });

        var maxHealth = System.Math.Max(1, config.MaxHealth);

        var health = enemy.AddComponent(new Health2DComponent(maxHealth)
        {
            DestroyEntityOnDeath = true
        });

        health.Damaged += (_, amount) =>
        {
            Console.WriteLine($"{config.Name} took {amount} damage. HP {health.CurrentHealth}/{health.MaxHealth}");
            config.OnDamaged?.Invoke(amount, transform.Position);
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
