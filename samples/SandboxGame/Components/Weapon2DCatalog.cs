using System.Text.Json;
using System.Text.Json.Serialization;
using Sindri.Graphics;

internal static class Weapon2DCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static List<Weapon2DDefinition> LoadOrCreateDefault(string path)
    {
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var weapons = JsonSerializer.Deserialize<List<Weapon2DDefinition>>(json, JsonOptions);

            if (weapons is not null && weapons.Count > 0)
            {
                Console.WriteLine($"Loaded weapons from {path}");
                return weapons;
            }
        }

        var defaults = CreateDefaults();
        Save(path, defaults);
        Console.WriteLine($"Created default weapons at {path}");
        return defaults;
    }

    public static void Save(string path, IReadOnlyList<Weapon2DDefinition> weapons)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(weapons, JsonOptions);
        File.WriteAllText(path, json);
    }

    private static List<Weapon2DDefinition> CreateDefaults()
    {
        return new List<Weapon2DDefinition>
        {
            new(
                Name: "Bow",
                AttackType: WeaponAttackType.Projectile,
                CooldownSeconds: 0.22f,
                ProjectileSpeed: 720f,
                ProjectileSpawnOffset: 34f,
                Damage: 1,
                ProjectileSize: 12f,
                ProjectileColor: ColorRGBA.White,
                MuzzleColor: ColorRGBA.SindriGold,
                MeleeRange: 48f,
                MeleeSize: 48f,
                MeleeLifetimeSeconds: 0.08f),

            new(
                Name: "Crossbow",
                AttackType: WeaponAttackType.Projectile,
                CooldownSeconds: 0.55f,
                ProjectileSpeed: 980f,
                ProjectileSpawnOffset: 36f,
                Damage: 2,
                ProjectileSize: 14f,
                ProjectileColor: ColorRGBA.SindriCyan,
                MuzzleColor: ColorRGBA.SindriCyan,
                MeleeRange: 48f,
                MeleeSize: 48f,
                MeleeLifetimeSeconds: 0.08f),

            new(
                Name: "Throwing Dagger",
                AttackType: WeaponAttackType.Projectile,
                CooldownSeconds: 0.12f,
                ProjectileSpeed: 620f,
                ProjectileSpawnOffset: 30f,
                Damage: 1,
                ProjectileSize: 8f,
                ProjectileColor: ColorRGBA.SindriGold,
                MuzzleColor: ColorRGBA.SindriGold,
                MeleeRange: 44f,
                MeleeSize: 36f,
                MeleeLifetimeSeconds: 0.06f),

            new(
                Name: "Sword",
                AttackType: WeaponAttackType.Melee,
                CooldownSeconds: 0.32f,
                ProjectileSpeed: 0f,
                ProjectileSpawnOffset: 0f,
                Damage: 2,
                ProjectileSize: 18f,
                ProjectileColor: ColorRGBA.SindriGold,
                MuzzleColor: ColorRGBA.SindriGold,
                MeleeRange: 46f,
                MeleeSize: 58f,
                MeleeLifetimeSeconds: 0.10f),

            new(
                Name: "Claws",
                AttackType: WeaponAttackType.Melee,
                CooldownSeconds: 0.16f,
                ProjectileSpeed: 0f,
                ProjectileSpawnOffset: 0f,
                Damage: 1,
                ProjectileSize: 10f,
                ProjectileColor: ColorRGBA.White,
                MuzzleColor: ColorRGBA.White,
                MeleeRange: 34f,
                MeleeSize: 44f,
                MeleeLifetimeSeconds: 0.07f)
        };
    }
}
