using Sindri.Behaviors2D.Components;
using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Core.Prefabs;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Physics2D.Components;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Scenes;
using Sindri.Renderer2D.Tilemaps;

internal sealed class SandboxScene : Scene2D
{
    private const string MapSavePath = "runtime-data/maps/sandbox.tilemap.json";
    private const string InputBindingsPath = "runtime-data/input/sandbox.actions.json";
    private const string WeaponCatalogPath = "runtime-data/weapons/sandbox.weapons.json";
    private const string LevelPath = "runtime-data/levels/sandbox.level.json";

    private const string ExitAction = "Exit";
    private const string RestartAction = "Restart";
    private const string PauseAction = "Pause";
    private const string FireAction = "Fire";
    private const string SaveMapAction = "SaveMap";
    private const string LoadMapAction = "LoadMap";
    private const string MoveLeftAction = "MoveLeft";
    private const string MoveRightAction = "MoveRight";
    private const string MoveUpAction = "MoveUp";
    private const string MoveDownAction = "MoveDown";
    private const string TeleportPlayerAction = "TeleportPlayer";
    private const string PaintTileAction = "PaintTile";
    private const string ToggleColliderDebugAction = "ToggleColliderDebug";

    private const float PlayerSize = 48f;
    private const float PlayerSpeed = 320f;

    private const string ZoomInAction = "ZoomIn";
    private const string ZoomOutAction = "ZoomOut";

    private const string CycleWeaponAction = "CycleWeapon";


private readonly PickupPrefab _pickupPrefab = new();
    private readonly TriggerZonePrefab _triggerZonePrefab = new();
    private readonly ProjectilePrefab _projectilePrefab = new();
    private readonly MeleeHitboxPrefab _meleeHitboxPrefab = new();
    private readonly TargetDummyPrefab _targetDummyPrefab = new();
    private readonly EnemyPrefab _enemyPrefab = new();

    private PrefabSpawner? _prefabSpawner;
    private IInputDevice? _keyboard;
    private IMouseDevice? _mouse;
    private InputActionMap? _actions;

    private TileMap2D? _map;
    private TileHover2DComponent? _tileHover;
    private Transform2D? _playerTransform;
    private Transform2D? _cameraTransform;
    private Health2DComponent? _playerHealth;
    private TextRenderer2D? _debugText;
    private TextRenderer2D? _hudText;
    private Entity? _pauseOverlay;
    private CameraShake2DComponent? _cameraShake;
    private PlayerWeapon2DComponent? _playerWeapon;

    private int _collectedPickups;
    private int _totalPickups;
    private bool _isInTriggerZone;
private int _destroyedDummies;
    private int _totalDummies;
    private int _defeatedEnemies;
    private int _totalEnemies;
    private bool _levelComplete;
    private bool _isPaused;
    private bool _showColliderDebug;

    protected override void OnEnter(SceneContext context)
    {
        _keyboard = context.Services.GetRequiredService<IInputDevice>();
        _mouse = context.Services.GetRequiredService<IMouseDevice>();
        _actions = context.Services.GetRequiredService<InputActionMap>();
        ConfigureInputActions(_actions);

        _prefabSpawner = new PrefabSpawner(this);

        BackgroundColor = ColorRGBA.Black;

        var mapInfo = CreateTileMap();
        _map = mapInfo.Map;

        var level = SandboxLevel2DLoader.LoadOrCreateDefault(LevelPath);
        var playerSpawn = level.PlayerSpawn ?? new SandboxSpawn2D { Name = "Player Spawn", X = 0f, Y = 0f };

        var player = CreatePlayer(mapInfo, new Vector2F(playerSpawn.X, playerSpawn.Y));
        var playerTransform = player.GetRequiredComponent<Transform2D>();

        CreateCamera(playerTransform, mapInfo.WorldBounds);
        AttachPlayerWeapon(player, mapInfo);
        CreateTileHover(mapInfo, _mouse);
        CreateAimReticle(_mouse);

        SpawnSandboxLevel(level);

        CreateDebugOverlay();
        CreateGameplayHud();
        CreatePauseOverlay();
        AddDebugColliderRenderersToExistingEntities();

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("WASD / Arrow Keys move player. Hold Space fires. R cycles weapons. Q/E zooms camera. Tab pauses. F toggles colliders. Enemies damage on contact. Left click teleports. Right click toggles solid tiles. P saves. O loads. ESC exits.");
        Console.WriteLine("Gray and red tiles are solid. Cyan zone is a trigger.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_actions?.WasPressed(ExitAction) == true)
        {
            Context?.RequestExit();
            return;
        }

        if (_actions?.WasPressed(PauseAction) == true)
        {
            _isPaused = !_isPaused;

            if (_pauseOverlay is not null)
            {
                _pauseOverlay.IsActive = _isPaused;
            }

            Console.WriteLine(_isPaused ? "Paused." : "Unpaused.");
        }

        if (_actions?.WasPressed(ToggleColliderDebugAction) == true)
        {
            ToggleColliderDebug();
        }

        if (_actions?.WasPressed(SaveMapAction) == true && _map is not null)
        {
            TileMapJsonSerializer.Save(_map, MapSavePath);
            Console.WriteLine($"Saved tilemap to {MapSavePath}");
        }

        if (_actions?.WasPressed(LoadMapAction) == true && _map is not null)
        {
            try
            {
                var loadedMap = TileMapJsonSerializer.Load(MapSavePath);
                _map.CopyFrom(loadedMap);
                Console.WriteLine($"Loaded tilemap from {MapSavePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load tilemap: {ex.Message}");
            }
        }

        UpdateDebugText();
        UpdateGameplayHud();

        if (!_isPaused && _tileHover?.WasTileClicked == true)
        {
            Console.WriteLine($"Clicked tile {_tileHover.ClickedTileX}, {_tileHover.ClickedTileY}");
        }
    }

    protected override bool ShouldUpdateEntities(SindriTime time)
    {
        return !_isPaused;
    }

    protected override void OnExit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }

    private Entity CreatePlayer(TileMapInfo mapInfo, Vector2F startPosition)
    {
        if (_actions is null)
        {
            throw new InvalidOperationException("Input actions were not initialized.");
        }

        var player = CreateEntity("Player");
        player.AddTag("Player");

        _playerTransform = player.AddComponent(new Transform2D
        {
            Position = startPosition
        });

        player.AddComponent(new BoxCollider2D(PlayerSize, PlayerSize));

        _playerHealth = player.AddComponent(new Health2DComponent(maxHealth: 10)
        {
            DestroyEntityOnDeath = false
        });

        _playerHealth.Damaged += (_, amount) =>
        {
            Console.WriteLine($"Player took {amount} damage. HP {_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}");

            _cameraShake?.Shake(0.16f, 10f);

            if (_playerTransform is not null)
            {
                SpawnParticleBurst(
                    _playerTransform.Position + new Vector2F(PlayerSize / 2f, PlayerSize / 2f),
                    ColorRGBA.SindriRed,
                    count: 18,
                    strength: 1.2f);

                SpawnFloatingText(
                    $"-{amount}",
                    _playerTransform.Position + new Vector2F(8f, -18f),
                    ColorRGBA.SindriRed);
            }
        };

        _playerHealth.Died += _ =>
        {
            Console.WriteLine("Player died.");
            Context?.ChangeScene(new GameOverScene());
        };

        player.AddComponent(new ActionMove2DComponent(_actions, PlayerSpeed)
        {
            MoveLeftAction = MoveLeftAction,
            MoveRightAction = MoveRightAction,
            MoveUpAction = MoveUpAction,
            MoveDownAction = MoveDownAction
        });

        player.AddComponent(new TileMapCollision2DComponent(mapInfo.Map)
        {
            MapWorldPosition = mapInfo.WorldPosition,
            UseAxisSeparation = true,
            MaxAxisResolveDistance = PlayerSize * 2f
        });

        player.AddComponent(new EntityCollision2DComponent(this)
        {
            TargetTag = "Solid",
            UseAxisSeparation = true,
            MaxAxisResolveDistance = PlayerSize * 2f
        });

        player.AddComponent(new RectangleRenderer2D(PlayerSize, 12f, new ColorRGBA(8, 10, 14))
        {
            Offset = new Vector2F(0f, PlayerSize - 4f),
            ClampToViewport = false,
            RenderLayer = 5,
            RenderOrder = 0
        });

        var bodyRenderer = player.AddComponent(new RectangleRenderer2D(PlayerSize, PlayerSize, ColorRGBA.SindriGold)
        {
            ClampToViewport = false,
            RenderLayer = 10,
            RenderOrder = 0
        });

        player.AddComponent(new DamageFlash2DComponent(_playerHealth, bodyRenderer)
        {
            FlashColor = ColorRGBA.White,
            FlashDurationSeconds = 0.12f
        });

        player.AddComponent(new HealthBar2DRenderer(_playerHealth)
        {
            Width = PlayerSize,
            Height = 6f,
            Offset = new Vector2F(0f, -12f),
            RenderLayer = 30
        });

        return player;
    }

    private void CreateCamera(Transform2D playerTransform, Rect2D worldBounds)
    {
        var cameraEntity = CreateEntity("Camera");

        _cameraTransform = cameraEntity.AddComponent(new Transform2D
        {
            Position = playerTransform.Position
        });

        ActiveCamera = cameraEntity.AddComponent(new Camera2D());
        _cameraShake = cameraEntity.AddComponent(new CameraShake2DComponent());

        cameraEntity.AddComponent(new CameraFollow2DComponent(playerTransform)
        {
            TargetOffset = new Vector2F(PlayerSize / 2f, PlayerSize / 2f),
            FollowStrength = 1f
        });

        cameraEntity.AddComponent(new CameraBounds2DComponent(worldBounds));

        if (_actions is null)
        {
            throw new InvalidOperationException("Input actions were not initialized.");
        }

        cameraEntity.AddComponent(new CameraZoom2DComponent(_actions)
        {
            ZoomInAction = ZoomInAction,
            ZoomOutAction = ZoomOutAction,
            MinZoom = 0.5f,
            MaxZoom = 2.5f,
            ZoomSpeed = 1.5f
        });
    }

    private void AttachPlayerWeapon(Entity player, TileMapInfo mapInfo)
    {
        if (_actions is null || _mouse is null || ActiveCamera is null || _prefabSpawner is null || _map is null)
        {
            throw new InvalidOperationException("Weapon dependencies were not initialized.");
        }

        var weapon = player.AddComponent(new PlayerWeapon2DComponent(
            _actions,
            _mouse,
            ActiveCamera,
            this,
            _prefabSpawner,
            _projectilePrefab,
            _meleeHitboxPrefab,
            _map,
            GetCurrentMapWorldPosition,
            onFired: (spawnPosition, firedWeapon) =>
            {
                SpawnParticleBurst(spawnPosition, firedWeapon.MuzzleColor, count: 8, strength: 0.55f);
                AddDebugColliderRenderersToExistingEntities();
            },
            onMeleeAttack: SpawnMeleeSlash)
        {
            FireAction = FireAction,
            CycleWeaponAction = CycleWeaponAction,
            OwnerWidth = PlayerSize,
            OwnerHeight = PlayerSize
        });

        _playerWeapon = weapon;

        foreach (var definition in Weapon2DCatalog.LoadOrCreateDefault(WeaponCatalogPath))
        {
            weapon.Weapons.Add(definition);
        }

        player.AddComponent(new EquippedWeaponVisual2DRenderer(weapon, _mouse, ActiveCamera)
        {
            OwnerWidth = PlayerSize,
            OwnerHeight = PlayerSize,
            HoldDistance = 34f,
            RenderLayer = 16
        });
    }

    private void CreateTileHover(TileMapInfo mapInfo, IMouseDevice mouse)
    {
        if (ActiveCamera is null || _playerTransform is null || _actions is null)
        {
            throw new InvalidOperationException("Camera/player/actions must exist before tile hover is created.");
        }

        var hoverEntity = CreateEntity("Tile Hover");

        _tileHover = hoverEntity.AddComponent(new TileHover2DComponent(mapInfo.Map, mouse, ActiveCamera)
        {
            MapWorldPosition = mapInfo.WorldPosition
        });

        hoverEntity.AddComponent(new ActionTilePaint2DComponent(mapInfo.Map, _tileHover, _actions)
        {
            PaintAction = PaintTileAction,
            SolidColor = ColorRGBA.SindriRed
        });

        hoverEntity.AddComponent(new TileHoverRenderer2D(_tileHover)
        {
            HoverColor = new ColorRGBA(214, 164, 74)
        });

        var player = _playerTransform.Entity
            ?? throw new InvalidOperationException("Player entity was not attached.");

        player.AddComponent(new ActionMouseClickTeleport2DComponent(_actions, mouse)
        {
            ActionName = TeleportPlayerAction,
            Camera = ActiveCamera,
            CenterOnMouse = true,
            CenterWidth = PlayerSize,
            CenterHeight = PlayerSize
        });
    }

    private void CreateDebugOverlay()
    {
        var debugOverlay = CreateEntity("Debug Overlay");

        debugOverlay.AddComponent(new Transform2D
        {
            Position = new Vector2F(12f, 12f)
        });

        _debugText = debugOverlay.AddComponent(new TextRenderer2D("Debug", ColorRGBA.White)
        {
            RenderSpace = RenderSpace.Screen,
            RenderLayer = 10_000
        });
    }

    private void CreateGameplayHud()
    {
        var hud = CreateEntity("Gameplay HUD");

        hud.AddComponent(new Transform2D
        {
            Position = new Vector2F(12f, 660f)
        });

        _hudText = hud.AddComponent(new TextRenderer2D("HUD", ColorRGBA.SindriGold)
        {
            RenderSpace = RenderSpace.Screen,
            RenderLayer = 10_001
        });
    }

    private void CreatePauseOverlay()
    {
        _pauseOverlay = CreateEntity("Pause Overlay");

        _pauseOverlay.AddComponent(new Transform2D
        {
            Position = new Vector2F(500f, 360f)
        });

        _pauseOverlay.AddComponent(new TextRenderer2D("PAUSED - Press Tab to resume", ColorRGBA.White)
        {
            RenderSpace = RenderSpace.Screen,
            RenderLayer = 20_000
        });

        _pauseOverlay.IsActive = false;
    }

    private void UpdateDebugText()
    {
        if (_debugText is null || _playerTransform is null || _cameraTransform is null)
        {
            return;
        }

        var tileText = " | Tile none";

        if (_tileHover?.HasHoveredTile == true && _map is not null)
        {
            var tile = _map.GetTile(_tileHover.HoveredTileX, _tileHover.HoveredTileY);
            var solidText = tile.IsSolid ? "solid" : "walkable";

            tileText = $" | Tile {_tileHover.HoveredTileX},{_tileHover.HoveredTileY} {solidText}";
        }

        var hpText = _playerHealth is null
            ? "HP none"
            : $"HP {_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}";

        _debugText.Text =
        $"Player {_playerTransform.Position.X:0},{_playerTransform.Position.Y:0} | " +
        $"Camera {_cameraTransform.Position.X:0},{_cameraTransform.Position.Y:0} Z{(ActiveCamera?.Zoom ?? 1f):0.00}" +
        tileText +
        $" | Goal {(_levelComplete ? "complete" : "active")}" +
        $" | Colliders {(_showColliderDebug ? "on" : "off")}";
    }

    private void UpdateGameplayHud()
    {
        if (_hudText is null)
        {
            return;
        }

        var hpText = _playerHealth is null
            ? "HP --/--"
            : $"HP {_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}";

        var weaponText = _playerWeapon is null
            ? "Weapon none"
            : $"Weapon {_playerWeapon.CurrentWeaponName}";

        var pickupText = $"Pickups {_collectedPickups}/{_totalPickups}";

        var targetText =
            $"Targets Dummies {_destroyedDummies}/{_totalDummies}  Enemies {_defeatedEnemies}/{_totalEnemies}";

        var zoneText = _isInTriggerZone
            ? "Zone inside"
            : "Zone outside";

        _hudText.Text =
            $"{hpText}   |   {weaponText}   |   {pickupText}   |   {targetText}   |   {zoneText}";
    }

    private void AddPickup(string name, float x, float y)
    {
        if (_prefabSpawner is null)
        {
            throw new InvalidOperationException("Prefab spawner was not initialized.");
        }

        _prefabSpawner.Spawn(
            _pickupPrefab,
            new PickupPrefabConfig(
                TriggerScene: this,
                Name: name,
                X: x,
                Y: y,
                OnCollected: () =>
                {
                    _collectedPickups++;
                    Console.WriteLine($"Collected {name}. {_collectedPickups}/{_totalPickups}");
                }));

        _totalPickups++;
    }

    private void AddTriggerZone(string name, float x, float y)
    {
        if (_prefabSpawner is null)
        {
            throw new InvalidOperationException("Prefab spawner was not initialized.");
        }

        _prefabSpawner.Spawn(
            _triggerZonePrefab,
            new TriggerZonePrefabConfig(
                TriggerScene: this,
                Name: name,
                X: x,
                Y: y,
                OnEntered: () =>
                {
                    _isInTriggerZone = true;
                    Console.WriteLine("Entered trigger zone.");
                },
                OnExited: () =>
                {
                    _isInTriggerZone = false;
                    Console.WriteLine("Exited trigger zone.");
                }));
    }

    private void AddTargetDummy(string name, float x, float y, int maxHealth = 3)
    {
        if (_prefabSpawner is null)
        {
            throw new InvalidOperationException("Prefab spawner was not initialized.");
        }

        _prefabSpawner.Spawn(
            _targetDummyPrefab,
            new TargetDummyPrefabConfig(
                Name: name,
                X: x,
                Y: y,
                MaxHealth: maxHealth <= 0 ? 3 : maxHealth,
                OnDamaged: (amount, position) =>
                {
                    _cameraShake?.Shake(0.06f, 3f);
                    SpawnParticleBurst(position + new Vector2F(24f, 24f), ColorRGBA.White, count: 12, strength: 0.8f);
                    SpawnFloatingText($"-{amount}", position + new Vector2F(8f, -18f), ColorRGBA.White);
                },
                OnDied: () =>
                {
                    _destroyedDummies++;
                    TryCompleteLevel();
                }));

        _totalDummies++;
    }

    private void AddEnemy(string name, float x, float y, int maxHealth = 4, float moveSpeed = 120f, int contactDamage = 1, float contactCooldownSeconds = 0.75f)
    {
        if (_prefabSpawner is null || _playerTransform is null || _map is null)
        {
            throw new InvalidOperationException("Enemy dependencies were not initialized.");
        }

        _prefabSpawner.Spawn(
            _enemyPrefab,
            new EnemyPrefabConfig(
                TriggerScene: this,
                Name: name,
                X: x,
                Y: y,
                Target: _playerTransform,
                TileMap: _map,
                MapWorldPosition: GetCurrentMapWorldPosition(),
                MaxHealth: maxHealth <= 0 ? 4 : maxHealth,
                MoveSpeed: moveSpeed <= 0f ? 120f : moveSpeed,
                ContactDamage: contactDamage <= 0 ? 1 : contactDamage,
                ContactCooldownSeconds: contactCooldownSeconds <= 0f ? 0.75f : contactCooldownSeconds,
                OnDamaged: (amount, position) =>
                {
                    _cameraShake?.Shake(0.06f, 3f);
                    SpawnParticleBurst(position + new Vector2F(24f, 24f), ColorRGBA.White, count: 12, strength: 0.8f);
                    SpawnFloatingText($"-{amount}", position + new Vector2F(8f, -18f), ColorRGBA.White);
                },
                OnDied: () =>
                {
                    _defeatedEnemies++;
                    TryCompleteLevel();
                }));

        _totalEnemies++;
    }

    private void SpawnSandboxLevel(SandboxLevel2DConfig level)
    {
        foreach (var pickup in level.Pickups)
        {
            AddPickup(pickup.Name, pickup.X, pickup.Y);
        }

        foreach (var triggerZone in level.TriggerZones)
        {
            AddTriggerZone(triggerZone.Name, triggerZone.X, triggerZone.Y);
        }

        foreach (var dummy in level.TargetDummies)
        {
            AddTargetDummy(dummy.Name, dummy.X, dummy.Y, dummy.MaxHealth);
        }

        foreach (var enemy in level.Enemies)
        {
            AddEnemy(enemy.Name, enemy.X, enemy.Y, enemy.MaxHealth, enemy.MoveSpeed, enemy.ContactDamage, enemy.ContactCooldownSeconds);
        }
    }

    private void TryCompleteLevel()
    {
        if (_levelComplete)
        {
            return;
        }

        if (_totalDummies <= 0 || _totalEnemies <= 0)
        {
            return;
        }

        if (_destroyedDummies < _totalDummies)
        {
            return;
        }

        if (_defeatedEnemies < _totalEnemies)
        {
            return;
        }

        _levelComplete = true;
        Console.WriteLine("All targets defeated. Victory.");
        Context?.ChangeScene(new VictoryScene());
    }

    private void CreateAimReticle(IMouseDevice mouse)
    {
        if (ActiveCamera is null)
        {
            throw new InvalidOperationException("Camera must exist before aim reticle is created.");
        }

        var reticle = CreateEntity("Aim Reticle");

        reticle.AddComponent(new AimReticle2DRenderer(mouse, ActiveCamera)
        {
            Color = ColorRGBA.SindriGold,
            Size = 28f,
            Thickness = 3f,
            DrawCenterDot = true,
            RenderLayer = 40_000
        });
    }

    private void SpawnParticleBurst(Vector2F position, ColorRGBA color, int count, float strength = 1f)
    {
        var entity = CreateEntity("Particle Burst");

        entity.AddComponent(new Transform2D
        {
            Position = position
        });

        entity.AddComponent(new ParticleBurst2DComponent(count, color)
        {
            MinSpeed = 45f * strength,
            MaxSpeed = 170f * strength,
            MinLifetimeSeconds = 0.2f,
            MaxLifetimeSeconds = 0.55f,
            MinSize = 3f,
            MaxSize = 7f,
            RenderLayer = 35
        });
    }

    private void SpawnMeleeSlash(Vector2F ownerCenter, Vector2F direction, Weapon2DDefinition weapon)
    {
        if (weapon.AttackType != WeaponAttackType.Melee)
        {
            return;
        }

        var slash = CreateEntity($"{weapon.Name} Slash");

        slash.AddComponent(new Transform2D
        {
            Position = ownerCenter + direction.Normalized() * 24f
        });

        slash.AddComponent(new MeleeSlashVisual2DComponent(weapon.ProjectileColor)
        {
            Direction = direction,
            Length = weapon.Name.Equals("Sword", StringComparison.OrdinalIgnoreCase) ? 82f : 52f,
            Thickness = weapon.Name.Equals("Sword", StringComparison.OrdinalIgnoreCase) ? 14f : 9f,
            LifetimeSeconds = weapon.MeleeLifetimeSeconds,
            RenderLayer = 26
        });
    }

    private void SpawnFloatingText(string text, Vector2F position, ColorRGBA color)
    {
        var entity = CreateEntity($"Floating Text {text}");

        entity.AddComponent(new Transform2D
        {
            Position = position
        });

        entity.AddComponent(new FloatingText2DComponent(text, color)
        {
            Velocity = new Vector2F(0f, -52f),
            LifetimeSeconds = 0.75f,
            RenderLayer = 10_000
        });
    }

    private void ToggleColliderDebug()
    {
        _showColliderDebug = !_showColliderDebug;

        foreach (var renderer in FindComponents<BoxColliderDebugRenderer2D>())
        {
            renderer.IsVisible = _showColliderDebug;
        }

        Console.WriteLine(_showColliderDebug ? "Collider debug enabled." : "Collider debug disabled.");
    }

    private void AddDebugColliderRenderersToExistingEntities()
    {
        foreach (var entity in GetActiveEntities())
        {
            EnsureDebugColliderRenderer(entity);
        }
    }

    private void EnsureDebugColliderRenderer(Entity entity)
    {
        if (entity.GetComponent<BoxCollider2D>() is null)
        {
            return;
        }

        if (entity.GetComponent<BoxColliderDebugRenderer2D>() is not null)
        {
            return;
        }

        entity.AddComponent(new BoxColliderDebugRenderer2D
        {
            IsVisible = _showColliderDebug,
            Color = ColorRGBA.SindriCyan,
            Thickness = 2f
        });
    }

    private Vector2F GetCurrentMapWorldPosition()
    {
        foreach (var entity in GetActiveEntities())
        {
            if (entity.Name == "Test Tilemap")
            {
                return entity.GetRequiredComponent<Transform2D>().Position;
            }
        }

        return Vector2F.Zero;
    }

    private TileMapInfo CreateTileMap()
    {
        const int tileSize = 64;
        const int width = 100;
        const int height = 80;

        var map = new TileMap2D(width, height, tileSize);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var checker = (x + y) % 2 == 0;

                map.SetTile(
                    x,
                    y,
                    checker
                        ? new ColorRGBA(32, 45, 58)
                        : new ColorRGBA(24, 34, 46));
            }
        }

        for (var x = 0; x < width; x++)
        {
            map.SetTile(x, 0, new ColorRGBA(80, 80, 88), isSolid: true);
            map.SetTile(x, height - 1, new ColorRGBA(80, 80, 88), isSolid: true);
        }

        for (var y = 0; y < height; y++)
        {
            map.SetTile(0, y, new ColorRGBA(80, 80, 88), isSolid: true);
            map.SetTile(width - 1, y, new ColorRGBA(80, 80, 88), isSolid: true);
        }

        for (var x = 10; x < 20; x++)
        {
            map.SetTile(x, 10, ColorRGBA.SindriRed, isSolid: true);
        }

        for (var y = 14; y < 26; y++)
        {
            map.SetTile(25, y, ColorRGBA.SindriRed, isSolid: true);
        }

        map.SetTile(8, 8, ColorRGBA.SindriGreen);
        map.SetTile(12, 6, ColorRGBA.White);
        map.SetTile(50, 40, ColorRGBA.SindriGreen);
        map.SetTile(70, 60, ColorRGBA.White);

        if (File.Exists(MapSavePath))
        {
            try
            {
                var loadedMap = TileMapJsonSerializer.Load(MapSavePath);

                if (loadedMap.Width == map.Width &&
                    loadedMap.Height == map.Height &&
                    loadedMap.TileSize == map.TileSize)
                {
                    map.CopyFrom(loadedMap);
                    Console.WriteLine($"Loaded startup tilemap from {MapSavePath}");
                }
                else
                {
                    Console.WriteLine($"Saved tilemap dimensions did not match current map. Using default map.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load startup tilemap: {ex.Message}");
            }
        }
        else
        {
            TileMapJsonSerializer.Save(map, MapSavePath);
            Console.WriteLine($"Created default startup tilemap at {MapSavePath}");
        }

        var worldPosition = new Vector2F(
            -width * tileSize / 2f,
            -height * tileSize / 2f);

        var worldBounds = new Rect2D(
            worldPosition.X,
            worldPosition.Y,
            width * tileSize,
            height * tileSize);

        var mapEntity = CreateEntity("Test Tilemap");

        mapEntity.AddComponent(new Transform2D
        {
            Position = worldPosition
        });

        mapEntity.AddComponent(new TileMapRenderer2D(map)
        {
            RenderLayer = -100
        });

        return new TileMapInfo(map, worldPosition, worldBounds);
    }

    private static void ConfigureInputActions(InputActionMap actions)
    {
        if (File.Exists(InputBindingsPath))
        {
            actions.Load(InputBindingsPath);

            var changed = false;

            if (!actions.HasAction(RestartAction))
            {
                actions.BindKey(RestartAction, Key.Enter);
                changed = true;
            }

            if (!actions.HasAction(ExitAction))
            {
                actions.BindKey(ExitAction, Key.Escape);
                changed = true;
            }

            if (!actions.HasAction(PauseAction))
            {
                actions.BindKey(PauseAction, Key.Tab);
                changed = true;
            }

            if (!actions.HasAction(ToggleColliderDebugAction))
            {
                actions.BindKey(ToggleColliderDebugAction, Key.F);
                changed = true;
            }

            if (!actions.HasAction(FireAction))
            {
                actions.BindKey(FireAction, Key.Space);
                changed = true;
            }

            if (!actions.HasAction(SaveMapAction))
            {
                actions.BindKey(SaveMapAction, Key.P);
                changed = true;
            }

            if (!actions.HasAction(LoadMapAction))
            {
                actions.BindKey(LoadMapAction, Key.O);
                changed = true;
            }

            if (!actions.HasAction(MoveLeftAction))
            {
                actions.BindKey(MoveLeftAction, Key.A);
                actions.BindKey(MoveLeftAction, Key.Left);
                changed = true;
            }

            if (!actions.HasAction(MoveRightAction))
            {
                actions.BindKey(MoveRightAction, Key.D);
                actions.BindKey(MoveRightAction, Key.Right);
                changed = true;
            }

            if (!actions.HasAction(MoveUpAction))
            {
                actions.BindKey(MoveUpAction, Key.W);
                actions.BindKey(MoveUpAction, Key.Up);
                changed = true;
            }

            if (!actions.HasAction(MoveDownAction))
            {
                actions.BindKey(MoveDownAction, Key.S);
                actions.BindKey(MoveDownAction, Key.Down);
                changed = true;
            }

            if (!actions.HasAction(TeleportPlayerAction))
            {
                actions.BindMouseButton(TeleportPlayerAction, MouseButton.Left);
                changed = true;
            }

            if (!actions.HasAction(PaintTileAction))
            {
                actions.BindMouseButton(PaintTileAction, MouseButton.Right);
                changed = true;
            }

            if (!actions.HasAction(ZoomInAction))
            {
                actions.BindKey(ZoomInAction, Key.E);
                changed = true;
            }

            if (!actions.HasAction(ZoomOutAction))
            {
                actions.BindKey(ZoomOutAction, Key.Q);
                changed = true;
            }

            if (!actions.HasAction(CycleWeaponAction))
            {
                actions.BindKey(CycleWeaponAction, Key.R);
                changed = true;
            }

            if (changed)
            {
                actions.Save(InputBindingsPath);
                Console.WriteLine($"Updated input bindings at {InputBindingsPath}");
            }
            else
            {
                Console.WriteLine($"Loaded input bindings from {InputBindingsPath}");
            }

            return;
        }

        ConfigureDefaultInputActions(actions);
        actions.Save(InputBindingsPath);
        Console.WriteLine($"Created default input bindings at {InputBindingsPath}");
    }

    private static void ConfigureDefaultInputActions(InputActionMap actions)
    {
        actions.Clear();

        actions.BindKey(ExitAction, Key.Escape);
        actions.BindKey(RestartAction, Key.Enter);
        actions.BindKey(PauseAction, Key.Tab);
        actions.BindKey(ToggleColliderDebugAction, Key.F);

        actions.BindKey(FireAction, Key.Space);
        actions.BindKey(SaveMapAction, Key.P);
        actions.BindKey(LoadMapAction, Key.O);

        actions.BindKey(MoveLeftAction, Key.A);
        actions.BindKey(MoveLeftAction, Key.Left);

        actions.BindKey(MoveRightAction, Key.D);
        actions.BindKey(MoveRightAction, Key.Right);

        actions.BindKey(MoveUpAction, Key.W);
        actions.BindKey(MoveUpAction, Key.Up);

        actions.BindKey(MoveDownAction, Key.S);
        actions.BindKey(MoveDownAction, Key.Down);

        actions.BindMouseButton(TeleportPlayerAction, MouseButton.Left);
        actions.BindMouseButton(PaintTileAction, MouseButton.Right);

        actions.BindKey(ZoomInAction, Key.E);
        actions.BindKey(ZoomOutAction, Key.Q);

        actions.BindKey(CycleWeaponAction, Key.R);
    }

    private readonly record struct TileMapInfo(TileMap2D Map, Vector2F WorldPosition, Rect2D WorldBounds);
}







