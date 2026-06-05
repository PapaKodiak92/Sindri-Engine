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

    private const float PlayerSize = 48f;
    private const float PlayerSpeed = 320f;

    private const string ExitAction = "Exit";
    private const string FireAction = "Fire";
    private const string SaveMapAction = "SaveMap";
    private const string LoadMapAction = "LoadMap";

    private readonly PickupPrefab _pickupPrefab = new();
    private readonly TriggerZonePrefab _triggerZonePrefab = new();
    private readonly ProjectilePrefab _projectilePrefab = new();
    private readonly TargetDummyPrefab _targetDummyPrefab = new();

    private PrefabSpawner? _prefabSpawner;
    private IInputDevice? _keyboard;
    private IMouseDevice? _mouse;
    private TileMap2D? _map;
    private TileHover2DComponent? _tileHover;
    private Transform2D? _playerTransform;
    private Transform2D? _cameraTransform;
    private TextRenderer2D? _debugText;
    private InputActionMap? _actions;

    private int _collectedPickups;
    private int _totalPickups;
    private bool _isInTriggerZone;
    private int _projectileCount;
    private int _destroyedDummies;
    private int _totalDummies;
    private int _defeatedEnemies;
    private int _totalEnemies;

    private readonly EnemyPrefab _enemyPrefab = new();

    private readonly CooldownTimer _fireCooldown = new(0.18f);

    private Health2DComponent? _playerHealth;

    protected override void OnEnter(SceneContext context)
    {
        _keyboard = context.Services.GetRequiredService<IInputDevice>();
        _mouse = context.Services.GetRequiredService<IMouseDevice>();
        _prefabSpawner = new PrefabSpawner(this);
        _actions = context.Services.GetRequiredService<InputActionMap>();
        ConfigureInputActions(_actions);

        BackgroundColor = ColorRGBA.Black;

        var mapInfo = CreateTileMap();
        _map = mapInfo.Map;

        var player = CreatePlayer(mapInfo);
        var playerTransform = player.GetRequiredComponent<Transform2D>();

        CreateCamera(playerTransform, mapInfo.WorldBounds);
        CreateTileHover(mapInfo, _mouse);

        AddPickup("Pickup A", -260f, -140f);
        AddPickup("Pickup B", 220f, 180f);
        AddPickup("Pickup C", 620f, -320f);
        AddPickup("Pickup D", -800f, 420f);

        AddTriggerZone("Test Trigger Zone", 420f, 320f);

        AddTargetDummy("Dummy A", 360f, -180f);
        AddTargetDummy("Dummy B", 720f, 240f);
        AddTargetDummy("Dummy C", -620f, -360f);

        AddEnemy("Enemy A", -420f, 260f);
        AddEnemy("Enemy B", 520f, -420f);
        AddEnemy("Enemy C", 900f, 380f);

        CreateDebugOverlay();

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("WASD / Arrow Keys move player. Hold Space fires. Enemies damage on contact. Left click teleports. Right click toggles solid tiles. P saves. O loads. ESC exits.");
        Console.WriteLine("Gray and red tiles are solid. Cyan zone is a trigger.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_actions?.WasPressed(ExitAction) == true)
        {
            Context?.RequestExit();
            return;
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

        _fireCooldown.Update(time);

        if (_actions?.IsDown(FireAction) == true && _fireCooldown.TryUse())
        {
            FireProjectileTowardMouse();
        }

        UpdateDebugText();

        if (_tileHover?.WasTileClicked == true)
        {
            Console.WriteLine($"Clicked tile {_tileHover.ClickedTileX}, {_tileHover.ClickedTileY}");
        }
    }

    protected override void OnExit()
    {
        Console.WriteLine("Sandbox scene exited.");
    }

    private Entity CreatePlayer(TileMapInfo mapInfo)
    {
        if (_keyboard is null)
        {
            throw new InvalidOperationException("Keyboard input was not initialized.");
        }

        var player = CreateEntity("Player");
        player.AddTag("Player");

        _playerTransform = player.AddComponent(new Transform2D
        {
            Position = Vector2F.Zero
        });

        player.AddComponent(new BoxCollider2D(PlayerSize, PlayerSize));

        _playerHealth = player.AddComponent(new Health2DComponent(maxHealth: 10)
        {
            DestroyEntityOnDeath = false
        });

        _playerHealth.Damaged += (_, amount) =>
        {
            Console.WriteLine($"Player took {amount} damage. HP {_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}");
            SpawnFloatingText($"-{amount}", _playerTransform.Position + new Vector2F(8f, -18f), ColorRGBA.SindriRed);
        };

        _playerHealth.Died += _ =>
        {
            Console.WriteLine("Player died.");
            Context?.RequestExit();
        };

        player.AddComponent(new KeyboardMove2DComponent(_keyboard, PlayerSpeed));

        player.AddComponent(new TileMapCollision2DComponent(mapInfo.Map)
        {
            MapWorldPosition = mapInfo.WorldPosition,
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

        cameraEntity.AddComponent(new CameraFollow2DComponent(playerTransform)
        {
            TargetOffset = new Vector2F(PlayerSize / 2f, PlayerSize / 2f),
            FollowStrength = 1f
        });

        cameraEntity.AddComponent(new CameraBounds2DComponent(worldBounds));
    }

    private void CreateTileHover(TileMapInfo mapInfo, IMouseDevice mouse)
    {
        if (ActiveCamera is null || _playerTransform is null)
        {
            throw new InvalidOperationException("Camera/player must exist before tile hover is created.");
        }

        var hoverEntity = CreateEntity("Tile Hover");

        _tileHover = hoverEntity.AddComponent(new TileHover2DComponent(mapInfo.Map, mouse, ActiveCamera)
        {
            MapWorldPosition = mapInfo.WorldPosition
        });

        hoverEntity.AddComponent(new TilePaint2DComponent(mapInfo.Map, _tileHover, mouse)
        {
            PaintButton = MouseButton.Right,
            SolidColor = ColorRGBA.SindriRed
        });

        hoverEntity.AddComponent(new TileHoverRenderer2D(_tileHover)
        {
            HoverColor = new ColorRGBA(214, 164, 74)
        });

        var player = _playerTransform.Entity
            ?? throw new InvalidOperationException("Player entity was not attached.");

        player.AddComponent(new MouseClickTeleport2DComponent(mouse)
        {
            Button = MouseButton.Left,
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
            $"{hpText} | " +
            $"Camera {_cameraTransform.Position.X:0},{_cameraTransform.Position.Y:0}" +
            tileText +
            $" | Pickups {_collectedPickups}/{_totalPickups}" +
            $" | Zone {(_isInTriggerZone ? "inside" : "outside")}" +
            $" | Shots {_projectileCount}" +
            $" | Dummies {_destroyedDummies}/{_totalDummies}" +
            $" | Enemies {_defeatedEnemies}/{_totalEnemies}";
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

    private void AddTargetDummy(string name, float x, float y)
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
            OnDamaged: (amount, position) =>
            {
                SpawnFloatingText($"-{amount}", position + new Vector2F(8f, -18f), ColorRGBA.White);
            },
            OnDied: () =>
                {
                    _destroyedDummies++;
                }));

        _totalDummies++;
    }

    private void FireProjectileTowardMouse()
    {
        if (_prefabSpawner is null || _playerTransform is null || _mouse is null || ActiveCamera is null || _map is null)
        {
            return;
        }

        var mouseScreen = new Vector2F(_mouse.Position.X, _mouse.Position.Y);
        var mouseWorld = ActiveCamera.ScreenToWorld(mouseScreen);

        var playerCenter = _playerTransform.Position + new Vector2F(PlayerSize / 2f, PlayerSize / 2f);
        var direction = (mouseWorld - playerCenter).Normalized();

        if (direction == Vector2F.Zero)
        {
            return;
        }

        const float projectileSpeed = 720f;
        const float projectileSpawnOffset = 34f;

        var spawnPosition = playerCenter + direction * projectileSpawnOffset;

        _projectileCount++;

        _prefabSpawner.Spawn(
            _projectilePrefab,
            new ProjectilePrefabConfig(
                TriggerScene: this,
                Name: $"Projectile {_projectileCount}",
                X: spawnPosition.X,
                Y: spawnPosition.Y,
                Velocity: direction * projectileSpeed,
                TileMap: _map,
                MapWorldPosition: GetCurrentMapWorldPosition(),
                Damage: 1));
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

    private void AddEnemy(string name, float x, float y)
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
            OnDamaged: (amount, position) =>
            {
                SpawnFloatingText($"-{amount}", position + new Vector2F(8f, -18f), ColorRGBA.White);
            },
            OnDied: () =>
                {
                    _defeatedEnemies++;
                }));

        _totalEnemies++;
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

    private static void ConfigureInputActions(InputActionMap actions)
    {
        actions.Clear();

        actions.BindKey(ExitAction, Key.Escape);
        actions.BindKey(FireAction, Key.Space);
        actions.BindKey(SaveMapAction, Key.P);
        actions.BindKey(LoadMapAction, Key.O);
    }

    private readonly record struct TileMapInfo(TileMap2D Map, Vector2F WorldPosition, Rect2D WorldBounds);
}
