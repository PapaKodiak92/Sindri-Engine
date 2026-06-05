using Sindri.Behaviors2D.Components;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Tilemaps;

internal sealed record ProjectilePrefabConfig(
    Scene TriggerScene,
    string Name,
    float X,
    float Y,
    Vector2F Velocity,
    TileMap2D TileMap,
    Vector2F MapWorldPosition,
    int Damage,
    float Size,
    ColorRGBA Color);

internal sealed class ProjectilePrefab : IPrefab<ProjectilePrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, ProjectilePrefabConfig config)
    {
        var projectileSize = System.MathF.Max(2f, config.Size);

        var projectile = spawner.SpawnEntity(config.Name);
        projectile.AddTag("Projectile");

        projectile.AddComponent(new Transform2D
        {
            Position = new Vector2F(config.X, config.Y)
        });

        projectile.AddComponent(new BoxCollider2D(projectileSize, projectileSize)
        {
            IsTrigger = true
        });

        projectile.AddComponent(new Velocity2DComponent
        {
            Velocity = config.Velocity
        });

        var trigger = projectile.AddComponent(new Trigger2DComponent(config.TriggerScene)
        {
            TargetTag = "Damageable"
        });

        projectile.AddComponent(new DamageOnTrigger2DComponent(trigger, config.Damage)
        {
            DestroySelfAfterHit = true
        });

        projectile.AddComponent(new DestroyOnSolidTile2DComponent(config.TileMap)
        {
            MapWorldPosition = config.MapWorldPosition,
            FallbackWidth = projectileSize,
            FallbackHeight = projectileSize
        });

        projectile.AddComponent(new DestroyAfterTimeComponent(2.5f));

        projectile.AddComponent(new RectangleRenderer2D(
            projectileSize,
            projectileSize,
            config.Color)
        {
            ClampToViewport = false,
            RenderLayer = 20
        });

        return projectile;
    }
}
