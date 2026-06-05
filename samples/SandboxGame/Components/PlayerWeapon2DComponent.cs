using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Core.Scenes;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Tilemaps;

internal sealed class PlayerWeapon2DComponent : Component
{
    private readonly InputActionMap _actions;
    private readonly IMouseDevice _mouse;
    private readonly Camera2D _camera;
    private readonly Scene _triggerScene;
    private readonly PrefabSpawner _prefabSpawner;
    private readonly ProjectilePrefab _projectilePrefab;
    private readonly MeleeHitboxPrefab _meleeHitboxPrefab;
    private readonly TileMap2D _tileMap;
    private readonly Func<Vector2F> _getMapWorldPosition;
    private readonly Action<Vector2F, Weapon2DDefinition>? _onFired;
    private readonly CooldownTimer _cooldown = new(0.18f);

    private int _weaponIndex;

    public PlayerWeapon2DComponent(
        InputActionMap actions,
        IMouseDevice mouse,
        Camera2D camera,
        Scene triggerScene,
        PrefabSpawner prefabSpawner,
        ProjectilePrefab projectilePrefab,
        MeleeHitboxPrefab meleeHitboxPrefab,
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
        _meleeHitboxPrefab = meleeHitboxPrefab ?? throw new ArgumentNullException(nameof(meleeHitboxPrefab));
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

    public int AttackCount { get; private set; }

    public Weapon2DDefinition CurrentWeaponDefinition => CurrentWeapon;

    public string CurrentWeaponName => CurrentWeapon.Name;

    private Weapon2DDefinition CurrentWeapon
    {
        get
        {
            if (Weapons.Count == 0)
            {
                return new Weapon2DDefinition(
                    Name: "Default Bow",
                    AttackType: WeaponAttackType.Projectile,
                    CooldownSeconds: 0.18f,
                    ProjectileSpeed: 720f,
                    ProjectileSpawnOffset: 34f,
                    Damage: 1,
                    ProjectileSize: 12f,
                    ProjectileColor: ColorRGBA.White,
                    MuzzleColor: ColorRGBA.SindriGold,
                    MeleeRange: 48f,
                    MeleeSize: 48f,
                    MeleeLifetimeSeconds: 0.08f);
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

        Attack(weapon);
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

    private void Attack(Weapon2DDefinition weapon)
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

        AttackCount++;

        if (weapon.AttackType == WeaponAttackType.Melee)
        {
            SpawnMeleeHitbox(weapon, ownerCenter, direction);
            return;
        }

        SpawnProjectile(weapon, ownerCenter, direction);
    }

    private void SpawnProjectile(Weapon2DDefinition weapon, Vector2F ownerCenter, Vector2F direction)
    {
        var spawnPosition = ownerCenter + direction * weapon.ProjectileSpawnOffset;

        _prefabSpawner.Spawn(
            _projectilePrefab,
            new ProjectilePrefabConfig(
                TriggerScene: _triggerScene,
                Name: $"{weapon.Name} Projectile {AttackCount}",
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

    private void SpawnMeleeHitbox(Weapon2DDefinition weapon, Vector2F ownerCenter, Vector2F direction)
    {
        var center = ownerCenter + direction * weapon.MeleeRange;
        var size = System.MathF.Max(4f, weapon.MeleeSize);

        _prefabSpawner.Spawn(
            _meleeHitboxPrefab,
            new MeleeHitboxPrefabConfig(
                TriggerScene: _triggerScene,
                Name: $"{weapon.Name} Hitbox {AttackCount}",
                X: center.X - size / 2f,
                Y: center.Y - size / 2f,
                Size: size,
                Damage: weapon.Damage,
                Color: weapon.ProjectileColor,
                LifetimeSeconds: weapon.MeleeLifetimeSeconds));

        _onFired?.Invoke(center, weapon);
    }
}
