using Sindri.Core.Entities;
using Sindri.Core.Math;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Renderer2D.Components;

internal sealed class EquippedWeaponVisual2DRenderer : RenderComponent
{
    private readonly PlayerProjectileWeapon2DComponent _weapon;
    private readonly IMouseDevice _mouse;
    private readonly Camera2D _camera;

    public EquippedWeaponVisual2DRenderer(
        PlayerProjectileWeapon2DComponent weapon,
        IMouseDevice mouse,
        Camera2D camera)
    {
        _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));

        RenderLayer = 16;
        RenderOrder = 0;
        RenderSpace = RenderSpace.World;
    }

    public float OwnerWidth { get; set; } = 48f;

    public float OwnerHeight { get; set; } = 48f;

    public float HoldDistance { get; set; } = 32f;

    public override void Render(IGraphicsDevice graphics)
    {
        if (Entity is null || Entity.IsDestroyed)
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
            direction = new Vector2F(1f, 0f);
        }

        var weapon = _weapon.CurrentWeaponDefinition;

        var size = System.Math.Clamp(weapon.ProjectileSize + 6f, 12f, 24f);
        var weaponCenter = ownerCenter + direction * HoldDistance;

        graphics.FillRectangle(
            new Rect2D(
                weaponCenter.X - size / 2f,
                weaponCenter.Y - size / 2f,
                size,
                size),
            weapon.ProjectileColor);

        graphics.FillRectangle(
            new Rect2D(
                weaponCenter.X - 3f,
                weaponCenter.Y - 3f,
                6f,
                6f),
            weapon.MuzzleColor);
    }
}
