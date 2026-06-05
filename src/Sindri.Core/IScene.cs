namespace Sindri.Core;

public interface IScene
{
    void Enter(SceneContext context);

    void Update(SindriTime time);

    void Exit();
}
