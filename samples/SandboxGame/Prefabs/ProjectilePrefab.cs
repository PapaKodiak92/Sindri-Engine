using Sindri.Behaviors2D.Components;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Tilemaps;

internal sealed record ProjectilePrefabConfig(
    string Name,
    float X,
    float Y,
    Vector2F Velocity,
    TileMap2D TileMap,
    Vector2F MapWorldPosition);

internal sealed class ProjectilePrefab : IPrefab<ProjectilePrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, ProjectilePrefabConfig config)
    {
        const float projectileSize = 12f;

        var projectile = spawner.SpawnEntity(config.Name);
        projectile.AddTag("Projectile");

        projectile.AddComponent(new Transform2D
        {
            Position = new Vector2F(config.X, config.Y)
        });

        projectile.AddComponent(new BoxCollider2D(projectileSize, projectileSize));

        projectile.AddComponent(new Velocity2DComponent
        {
            Velocity = config.Velocity
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
            ColorRGBA.White)
        {
            ClampToViewport = false,
            RenderLayer = 20
        });

        return projectile;
    }
}
