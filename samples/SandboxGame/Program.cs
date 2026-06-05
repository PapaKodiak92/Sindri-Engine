using Sindri.Behaviors2D.Components;
using Sindri.Core;
using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Physics2D.Components;
using Sindri.Platform.Windows;
using Sindri.Renderer2D.Components;
using Sindri.Renderer2D.Scenes;
using Sindri.Renderer2D.Tilemaps;

return WindowsGameRunner.Run(new SandboxGame());

internal sealed class SandboxGame : SindriGame
{
    public override string Name => "Sindri Sandbox";

    public override void Configure(EngineConfig config)
    {
        config.WindowTitle = Name;
        config.TargetWidth = 1280;
        config.TargetHeight = 720;
        config.LimitFrameRate = true;
        config.TargetFramesPerSecond = 60;
        config.ShowFrameRateInTitle = true;
    }

    public override IScene CreateInitialScene()
    {
        return new SandboxScene();
    }
}

internal sealed class SandboxScene : Scene2D
{
    private const string MapSavePath = "runtime-data/maps/sandbox.tilemap.json";

    private const float PlayerSize = 48f;
    private const float PlayerSpeed = 320f;

    private IInputDevice? _keyboard;
    private TileMap2D? _map;
    private TileHover2DComponent? _tileHover;
    private Transform2D? _playerTransform;
    private Transform2D? _cameraTransform;
    private TextRenderer2D? _debugText;

    private int _collectedPickups;
    private int _totalPickups;
    private bool _isInTriggerZone;

    protected override void OnEnter(SceneContext context)
    {
        _keyboard = context.Services.GetRequiredService<IInputDevice>();
        var mouse = context.Services.GetRequiredService<IMouseDevice>();

        BackgroundColor = ColorRGBA.Black;

        var mapInfo = CreateTileMap();
        _map = mapInfo.Map;

        var player = CreateEntity("Player");
        player.AddTag("Player");

        _playerTransform = player.AddComponent(new Transform2D
        {
            Position = Vector2F.Zero
        });

        player.AddComponent(new BoxCollider2D(PlayerSize, PlayerSize));

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

        player.AddComponent(new RectangleRenderer2D(PlayerSize, PlayerSize, ColorRGBA.SindriGold)
        {
            ClampToViewport = false,
            RenderLayer = 10,
            RenderOrder = 0
        });

        var cameraEntity = CreateEntity("Camera");

        _cameraTransform = cameraEntity.AddComponent(new Transform2D
        {
            Position = _playerTransform.Position
        });

        ActiveCamera = cameraEntity.AddComponent(new Camera2D());

        cameraEntity.AddComponent(new CameraFollow2DComponent(_playerTransform)
        {
            TargetOffset = new Vector2F(PlayerSize / 2f, PlayerSize / 2f),
            FollowStrength = 1f
        });

        cameraEntity.AddComponent(new CameraBounds2DComponent(mapInfo.WorldBounds));

        player.AddComponent(new MouseClickTeleport2DComponent(mouse)
        {
            Button = MouseButton.Left,
            Camera = ActiveCamera,
            CenterOnMouse = true,
            CenterWidth = PlayerSize,
            CenterHeight = PlayerSize
        });

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

        AddPickup("Pickup A", -260f, -140f);
        AddPickup("Pickup B", 220f, 180f);
        AddPickup("Pickup C", 620f, -320f);
        AddPickup("Pickup D", -800f, 420f);

        AddTriggerZone("Test Trigger Zone", 420f, 320f);

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

        Console.WriteLine("Sandbox scene entered.");
        Console.WriteLine("WASD / Arrow Keys move player. Left click teleports. Right click toggles solid tiles. P saves. O loads. ESC exits.");
        Console.WriteLine("Gray and red tiles are solid. Cyan zone is a trigger.");
    }

    protected override void OnUpdate(SindriTime time)
    {
        if (_keyboard?.WasKeyPressed(Key.Escape) == true)
        {
            Context?.RequestExit();
            return;
        }

        if (_keyboard?.WasKeyPressed(Key.P) == true && _map is not null)
        {
            TileMapJsonSerializer.Save(_map, MapSavePath);
            Console.WriteLine($"Saved tilemap to {MapSavePath}");
        }

        if (_keyboard?.WasKeyPressed(Key.O) == true && _map is not null)
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

        if (_tileHover?.WasTileClicked == true)
        {
            Console.WriteLine($"Clicked tile {_tileHover.ClickedTileX}, {_tileHover.ClickedTileY}");
        }
    }

    protected override void OnExit()
    {
        Console.WriteLine("Sandbox scene exited.");
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

        _debugText.Text =
            $"Player {_playerTransform.Position.X:0},{_playerTransform.Position.Y:0} | " +
            $"Camera {_cameraTransform.Position.X:0},{_cameraTransform.Position.Y:0}" +
            tileText +
            $" | Pickups {_collectedPickups}/{_totalPickups}" +
            $" | Zone {(_isInTriggerZone ? "inside" : "outside")}";
    }

    private void AddPickup(string name, float x, float y)
    {
        const float pickupSize = 32f;

        var pickup = CreateEntity(name);

        pickup.AddComponent(new Transform2D
        {
            Position = new Vector2F(x, y)
        });

        pickup.AddComponent(new BoxCollider2D(pickupSize, pickupSize)
        {
            IsTrigger = true
        });

        var trigger = pickup.AddComponent(new Trigger2DComponent(this)
        {
            TargetTag = "Player"
        });

        var pickupComponent = pickup.AddComponent(new Pickup2DComponent(trigger));

        pickupComponent.Collected += _ =>
        {
            _collectedPickups++;
            Console.WriteLine($"Collected {name}. {_collectedPickups}/{_totalPickups}");
        };

        pickup.AddComponent(new RectangleRenderer2D(pickupSize, pickupSize, ColorRGBA.SindriGreen)
        {
            ClampToViewport = false,
            RenderLayer = 8
        });

        _totalPickups++;
    }

    private void AddTriggerZone(string name, float x, float y)
    {
        const float zoneWidth = 160f;
        const float zoneHeight = 120f;

        var zone = CreateEntity(name);

        zone.AddComponent(new Transform2D
        {
            Position = new Vector2F(x, y)
        });

        zone.AddComponent(new BoxCollider2D(zoneWidth, zoneHeight)
        {
            IsTrigger = true
        });

        var trigger = zone.AddComponent(new Trigger2DComponent(this)
        {
            TargetTag = "Player"
        });

        trigger.Entered += _ =>
        {
            _isInTriggerZone = true;
            Console.WriteLine("Entered trigger zone.");
        };

        trigger.Exited += _ =>
        {
            _isInTriggerZone = false;
            Console.WriteLine("Exited trigger zone.");
        };

        zone.AddComponent(new RectangleRenderer2D(zoneWidth, zoneHeight, ColorRGBA.SindriCyan)
        {
            ClampToViewport = false,
            RenderLayer = 3,
            RenderOrder = 0
        });
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

    private readonly record struct TileMapInfo(TileMap2D Map, Vector2F WorldPosition, Rect2D WorldBounds);
}
