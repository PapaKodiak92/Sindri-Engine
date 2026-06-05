using Sindri.Behaviors2D.Components;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;

internal sealed record PickupPrefabConfig(
    Scene TriggerScene,
    string Name,
    float X,
    float Y,
    Action? OnCollected);

internal sealed class PickupPrefab : IPrefab<PickupPrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, PickupPrefabConfig config)
    {
        const float pickupSize = 32f;

        var pickup = spawner.SpawnEntity(config.Name);

        pickup.AddComponent(new Transform2D
        {
            Position = new Vector2F(config.X, config.Y)
        });

        pickup.AddComponent(new BoxCollider2D(pickupSize, pickupSize)
        {
            IsTrigger = true
        });

        var trigger = pickup.AddComponent(new Trigger2DComponent(config.TriggerScene)
        {
            TargetTag = "Player"
        });

        var pickupComponent = pickup.AddComponent(new Pickup2DComponent(trigger));
        pickupComponent.Collected += _ => config.OnCollected?.Invoke();

        pickup.AddComponent(new RectangleRenderer2D(pickupSize, pickupSize, ColorRGBA.SindriGreen)
        {
            ClampToViewport = false,
            RenderLayer = 8
        });

        return pickup;
    }
}

internal sealed record TriggerZonePrefabConfig(
    Scene TriggerScene,
    string Name,
    float X,
    float Y,
    Action? OnEntered,
    Action? OnExited);

internal sealed class TriggerZonePrefab : IPrefab<TriggerZonePrefabConfig>
{
    public Entity Create(IEntitySpawner spawner, TriggerZonePrefabConfig config)
    {
        const float zoneWidth = 160f;
        const float zoneHeight = 120f;

        var zone = spawner.SpawnEntity(config.Name);

        zone.AddComponent(new Transform2D
        {
            Position = new Vector2F(config.X, config.Y)
        });

        zone.AddComponent(new BoxCollider2D(zoneWidth, zoneHeight)
        {
            IsTrigger = true
        });

        var trigger = zone.AddComponent(new Trigger2DComponent(config.TriggerScene)
        {
            TargetTag = "Player"
        });

        trigger.Entered += _ => config.OnEntered?.Invoke();
        trigger.Exited += _ => config.OnExited?.Invoke();

        zone.AddComponent(new RectangleRenderer2D(zoneWidth, zoneHeight, ColorRGBA.SindriCyan)
        {
            ClampToViewport = false,
            RenderLayer = 3,
            RenderOrder = 0
        });

        return zone;
    }

    internal sealed record ProjectilePrefabConfig(
    string Name,
    float X,
    float Y,
    Sindri.Core.Math.Vector2F Velocity,
    Sindri.Renderer2D.Tilemaps.TileMap2D TileMap,
    Sindri.Core.Math.Vector2F MapWorldPosition);

    internal sealed class ProjectilePrefab : Sindri.Core.Prefabs.IPrefab<ProjectilePrefabConfig>
    {
        public Sindri.Core.Entities.Entity Create(Sindri.Core.Entities.IEntitySpawner spawner, ProjectilePrefabConfig config)
        {
            const float projectileSize = 12f;

            var projectile = spawner.SpawnEntity(config.Name);
            projectile.AddTag("Projectile");

            projectile.AddComponent(new Sindri.Core.Entities.Transform2D
            {
                Position = new Sindri.Core.Math.Vector2F(config.X, config.Y)
            });

            projectile.AddComponent(new Sindri.Physics2D.Components.BoxCollider2D(projectileSize, projectileSize));

            projectile.AddComponent(new Sindri.Physics2D.Components.Velocity2DComponent
            {
                Velocity = config.Velocity
            });

            projectile.AddComponent(new Sindri.Behaviors2D.Components.DestroyOnSolidTile2DComponent(config.TileMap)
            {
                MapWorldPosition = config.MapWorldPosition,
                FallbackWidth = projectileSize,
                FallbackHeight = projectileSize
            });

            projectile.AddComponent(new Sindri.Behaviors2D.Components.DestroyAfterTimeComponent(2.5f));

            projectile.AddComponent(new Sindri.Renderer2D.Components.RectangleRenderer2D(
                projectileSize,
                projectileSize,
                Sindri.Graphics.ColorRGBA.White)
            {
                ClampToViewport = false,
                RenderLayer = 20
            });

            return projectile;
        }
    }
}
