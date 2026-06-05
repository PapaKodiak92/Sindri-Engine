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
    private readonly Action<Vector2F, Weapon2DDefinition>? _onFired;
    private readonly CooldownTimer _cooldown = new(0.18f);

    private int _weaponIndex;

    public PlayerProjectileWeapon2DComponent(
        InputActionMap actions,
        IMouseDevice mouse,
        Camera2D camera,
        Scene triggerScene,
        PrefabSpawner prefabSpawner,
        ProjectilePrefab projectilePrefab,
        TileMap2D tileMap,
        Func<Vector2F> getMapWorldPosition,
        Action<Vector2F, Weapon2DDefinition>? onFired)
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

        UpdateOrder = ComponentUpdateOrder.Gameplay;
    }

    public List<Weapon2DDefinition> Weapons { get; } = new();

    public string FireAction { get; set; } = "Fire";

    public string CycleWeaponAction { get; set; } = "CycleWeapon";

    public float OwnerWidth { get; set; } = 48f;

    public float OwnerHeight { get; set; } = 48f;

    public int ShotsFired { get; private set; }

    public string CurrentWeaponName => CurrentWeapon.Name;

    private Weapon2DDefinition CurrentWeapon
    {
        get
        {
            if (Weapons.Count == 0)
            {
                return new Weapon2DDefinition(
                    Name: "Default",
                    CooldownSeconds: 0.18f,
                    ProjectileSpeed: 720f,
                    ProjectileSpawnOffset: 34f,
                    Damage: 1,
                    ProjectileSize: 12f,
                    ProjectileColor: ColorRGBA.White,
                    MuzzleColor: ColorRGBA.SindriGold);
            }

            _weaponIndex = System.Math.Clamp(_weaponIndex, 0, Weapons.Count - 1);
            return Weapons[_weaponIndex];
        }
    }

    public override void Update(SindriTime time)
    {
        if (Entity is null || Entity.IsDestroyed)
        {
            return;
        }

        if (_actions.WasPressed(CycleWeaponAction))
        {
            CycleNextWeapon();
        }

        _cooldown.Update(time);

        if (!_actions.IsDown(FireAction))
        {
            return;
        }

        var weapon = CurrentWeapon;
        _cooldown.DurationSeconds = weapon.CooldownSeconds;

        if (!_cooldown.TryUse())
        {
            return;
        }

        Fire(weapon);
    }

    private void CycleNextWeapon()
    {
        if (Weapons.Count <= 1)
        {
            return;
        }

        _weaponIndex = (_weaponIndex + 1) % Weapons.Count;
        _cooldown.Reset();

        Console.WriteLine($"Equipped weapon: {CurrentWeapon.Name}");
    }

    private void Fire(Weapon2DDefinition weapon)
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

        var spawnPosition = ownerCenter + direction * weapon.ProjectileSpawnOffset;

        ShotsFired++;

        _prefabSpawner.Spawn(
            _projectilePrefab,
            new ProjectilePrefabConfig(
                TriggerScene: _triggerScene,
                Name: $"{weapon.Name} Projectile {ShotsFired}",
                X: spawnPosition.X,
                Y: spawnPosition.Y,
                Velocity: direction * weapon.ProjectileSpeed,
                TileMap: _tileMap,
                MapWorldPosition: _getMapWorldPosition(),
                Damage: weapon.Damage,
                Size: weapon.ProjectileSize,
                Color: weapon.ProjectileColor));

        _onFired?.Invoke(spawnPosition, weapon);
    }
}
