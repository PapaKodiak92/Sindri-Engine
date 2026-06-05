using Sindri.Graphics;

internal enum WeaponAttackType
{
    Projectile,
    Melee
}

internal sealed record Weapon2DDefinition(
    string Name,
    WeaponAttackType AttackType,
    float CooldownSeconds,
    float ProjectileSpeed,
    float ProjectileSpawnOffset,
    int Damage,
    float ProjectileSize,
    ColorRGBA ProjectileColor,
    ColorRGBA MuzzleColor,
    float MeleeRange,
    float MeleeSize,
    float MeleeLifetimeSeconds);
