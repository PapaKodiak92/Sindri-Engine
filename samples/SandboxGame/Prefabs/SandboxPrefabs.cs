using Sindri.Behaviors2D.Components;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;

internal static class SandboxPrefabs
{
    public static Entity CreatePickup(
        IEntitySpawner spawner,
        Scene triggerScene,
        string name,
        float x,
        float y,
        Action? onCollected)
    {
        const float pickupSize = 32f;

        var pickup = spawner.SpawnEntity(name);

        pickup.AddComponent(new Transform2D
        {
            Position = new Vector2F(x, y)
        });

        pickup.AddComponent(new BoxCollider2D(pickupSize, pickupSize)
        {
            IsTrigger = true
        });

        var trigger = pickup.AddComponent(new Trigger2DComponent(triggerScene)
        {
            TargetTag = "Player"
        });

        var pickupComponent = pickup.AddComponent(new Pickup2DComponent(trigger));

        pickupComponent.Collected += _ => onCollected?.Invoke();

        pickup.AddComponent(new RectangleRenderer2D(pickupSize, pickupSize, ColorRGBA.SindriGreen)
        {
            ClampToViewport = false,
            RenderLayer = 8
        });

        return pickup;
    }

    public static Entity CreateTriggerZone(
        IEntitySpawner spawner,
        Scene triggerScene,
        string name,
        float x,
        float y,
        Action? onEntered,
        Action? onExited)
    {
        const float zoneWidth = 160f;
        const float zoneHeight = 120f;

        var zone = spawner.SpawnEntity(name);

        zone.AddComponent(new Transform2D
        {
            Position = new Vector2F(x, y)
        });

        zone.AddComponent(new BoxCollider2D(zoneWidth, zoneHeight)
        {
            IsTrigger = true
        });

        var trigger = zone.AddComponent(new Trigger2DComponent(triggerScene)
        {
            TargetTag = "Player"
        });

        trigger.Entered += _ => onEntered?.Invoke();
        trigger.Exited += _ => onExited?.Invoke();

        zone.AddComponent(new RectangleRenderer2D(zoneWidth, zoneHeight, ColorRGBA.SindriCyan)
        {
            ClampToViewport = false,
            RenderLayer = 3,
            RenderOrder = 0
        });

        return zone;
    }
}
