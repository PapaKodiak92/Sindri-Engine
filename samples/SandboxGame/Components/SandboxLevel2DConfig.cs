using System.Text.Json;

internal static class SandboxLevel2DLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static SandboxLevel2DConfig LoadOrCreateDefault(string path)
    {
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var level = JsonSerializer.Deserialize<SandboxLevel2DConfig>(json, JsonOptions);

            if (level is not null)
            {
                var changed = EnsureRequiredDefaults(level);

                if (changed)
                {
                    Save(path, level);
                    Console.WriteLine($"Updated level defaults at {path}");
                }
                else
                {
                    Console.WriteLine($"Loaded level from {path}");
                }

                return level;
            }
        }

        var defaults = CreateDefaults();
        Save(path, defaults);
        Console.WriteLine($"Created default level at {path}");
        return defaults;
    }

    public static void Save(string path, SandboxLevel2DConfig level)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(level, JsonOptions);
        File.WriteAllText(path, json);
    }

    private static bool EnsureRequiredDefaults(SandboxLevel2DConfig level)
    {
        var changed = false;

        if (level.PlayerSpawn is null)
        {
            level.PlayerSpawn = new SandboxSpawn2D
            {
                Name = "Player Spawn",
                X = 0f,
                Y = 0f
            };

            changed = true;
        }

        if (level.PlayerMaxHealth <= 0)
        {
            level.PlayerMaxHealth = 10;
            changed = true;
        }

        foreach (var dummy in level.TargetDummies)
        {
            if (dummy.MaxHealth <= 0)
            {
                dummy.MaxHealth = 3;
                changed = true;
            }
        }

        foreach (var enemy in level.Enemies)
        {
            if (enemy.MaxHealth <= 0)
            {
                enemy.MaxHealth = 4;
                changed = true;
            }

            if (enemy.MoveSpeed <= 0f)
            {
                enemy.MoveSpeed = 120f;
                changed = true;
            }

            if (enemy.ContactDamage <= 0)
            {
                enemy.ContactDamage = 1;
                changed = true;
            }

            if (enemy.ContactCooldownSeconds <= 0f)
            {
                enemy.ContactCooldownSeconds = 0.75f;
                changed = true;
            }
        }

        return changed;
    }

    private static SandboxLevel2DConfig CreateDefaults()
    {
        return new SandboxLevel2DConfig
        {
            PlayerMaxHealth = 10,

            PlayerSpawn = new SandboxSpawn2D
            {
                Name = "Player Spawn",
                X = 0f,
                Y = 0f
            },

            Pickups =
            {
                new() { Name = "Pickup A", X = -260f, Y = -140f },
                new() { Name = "Pickup B", X = 220f, Y = 180f },
                new() { Name = "Pickup C", X = 620f, Y = -320f },
                new() { Name = "Pickup D", X = -800f, Y = 420f }
            },

            TriggerZones =
            {
                new() { Name = "Test Trigger Zone", X = 420f, Y = 320f }
            },

            TargetDummies =
            {
                new() { Name = "Dummy A", X = 360f, Y = -180f, MaxHealth = 3 },
                new() { Name = "Dummy B", X = 720f, Y = 240f, MaxHealth = 3 },
                new() { Name = "Dummy C", X = -620f, Y = -360f, MaxHealth = 3 }
            },

            Enemies =
            {
                new()
                {
                    Name = "Enemy A",
                    X = -420f,
                    Y = 260f,
                    MaxHealth = 4,
                    MoveSpeed = 120f,
                    ContactDamage = 1,
                    ContactCooldownSeconds = 0.75f
                },

                new()
                {
                    Name = "Enemy B",
                    X = 520f,
                    Y = -420f,
                    MaxHealth = 4,
                    MoveSpeed = 120f,
                    ContactDamage = 1,
                    ContactCooldownSeconds = 0.75f
                },

                new()
                {
                    Name = "Enemy C",
                    X = 900f,
                    Y = 380f,
                    MaxHealth = 4,
                    MoveSpeed = 120f,
                    ContactDamage = 1,
                    ContactCooldownSeconds = 0.75f
                }
            }
        };
    }
}

internal sealed class SandboxLevel2DConfig
{
    public int PlayerMaxHealth { get; set; }

    public SandboxSpawn2D? PlayerSpawn { get; set; }

    public List<SandboxSpawn2D> Pickups { get; set; } = new();

    public List<SandboxSpawn2D> TriggerZones { get; set; } = new();

    public List<SandboxActorSpawn2D> TargetDummies { get; set; } = new();

    public List<SandboxActorSpawn2D> Enemies { get; set; } = new();
}

internal class SandboxSpawn2D
{
    public string Name { get; set; } = "Spawn";

    public float X { get; set; }

    public float Y { get; set; }
}

internal sealed class SandboxActorSpawn2D : SandboxSpawn2D
{
    public int MaxHealth { get; set; }

    public float MoveSpeed { get; set; }

    public int ContactDamage { get; set; }

    public float ContactCooldownSeconds { get; set; }
}
