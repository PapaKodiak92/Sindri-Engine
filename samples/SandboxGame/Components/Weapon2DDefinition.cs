using Sindri.Graphics;

internal sealed record Weapon2DDefinition(
    string Name,
    float CooldownSeconds,
    float ProjectileSpeed,
    float ProjectileSpawnOffset,
    int Damage,
    float ProjectileSize,
    ColorRGBA ProjectileColor,
    ColorRGBA MuzzleColor);
