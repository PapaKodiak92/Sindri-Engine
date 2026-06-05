using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Tilemaps;

internal sealed class PlayerProjectileWeapon2DComponent : Component
{
    private readonly InputActionMap _actions;
    private readonly IMouseDevice _mouse;
    private readonly Camera2D _camera;
    private readonly Scene _triggerScene;
    private readonly PrefabSpawner _prefabSpawner;
    private readonly ProjectilePrefab _projectilePrefab;
    private readonly TileMap2D _tileMap;
    private readonly Func<Vector2F> _getMapWorldPosition;
    private readonly Action<Vector2F>? _onFired;
    private readonly CooldownTimer _cooldown;

    public PlayerProjectileWeapon2DComponent(
        InputActionMap actions,
        IMouseDevice mouse,
        Camera2D camera,
        Scene triggerScene,
        PrefabSpawner prefabSpawner,
        ProjectilePrefab projectilePrefab,
        TileMap2D tileMap,
        Func<Vector2F> getMapWorldPosition,
        Action<Vector2F>? onFired)
    {
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _triggerScene = triggerScene ?? throw new ArgumentNullException(nameof(triggerScene));
        _prefabSpawner = prefabSpawner ?? throw new ArgumentNullException(nameof(prefabSpawner));
        _projectilePrefab = projectilePrefab ?? throw new ArgumentNullException(nameof(projectilePrefab));
        _tileMap = tileMap ?? throw new ArgumentNullException(nameof(tileMap));
        _getMapWorldPosition = getMapWorldPosition ?? throw new ArgumentNullException(nameof(getMapWorldPosition));
        _onFired = onFired;

        _cooldown = new CooldownTimer(0.18f);
        UpdateOrder = ComponentUpdateOrder.Gameplay;
    }

    public string FireAction { get; set; } = "Fire";

    public float OwnerWidth { get; set; } = 48f;

    public float OwnerHeight { get; set; } = 48f;

    public float ProjectileSpeed { get; set; } = 720f;

    public float ProjectileSpawnOffset { get; set; } = 34f;

    public float FireCooldownSeconds
    {
        get => _cooldown.DurationSeconds;
        set => _cooldown.DurationSeconds = value;
    }

    public int Damage { get; set; } = 1;

    public int ShotsFired { get; private set; }

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        _cooldown.Update(time);

        if (!_actions.IsDown(FireAction))
        {
            return;
        }

        if (!_cooldown.TryUse())
        {
            return;
        }

        Fire();
    }

    private void Fire()
    {
        if (Entity is null)
        {
            return;
        }

        var transform = Entity.GetRequiredComponent<Transform2D>();

        var mouseScreen = new Vector2F(_mouse.Position.X, _mouse.Position.Y);
        var mouseWorld = _camera.ScreenToWorld(mouseScreen);

        var ownerCenter = transform.Position + new Vector2F(OwnerWidth / 2f, OwnerHeight / 2f);
        var direction = (mouseWorld - ownerCenter).Normalized();

        if (direction == Vector2F.Zero)
        {
            return;
        }

        var spawnPosition = ownerCenter + direction * ProjectileSpawnOffset;

        ShotsFired++;

        _prefabSpawner.Spawn(
            _projectilePrefab,
            new ProjectilePrefabConfig(
                TriggerScene: _triggerScene,
                Name: $"Projectile {ShotsFired}",
                X: spawnPosition.X,
                Y: spawnPosition.Y,
                Velocity: direction * ProjectileSpeed,
                TileMap: _tileMap,
                MapWorldPosition: _getMapWorldPosition(),
                Damage: Damage));

        _onFired?.Invoke(spawnPosition);
    }
}
