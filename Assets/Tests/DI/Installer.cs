using HeavenInject;

public class Installer : LifetimeScope {
    protected override void Bind(IContainerBuilder builder) {
        builder.Register<ISceneLoader, SceneLoader>(LifeTime.Singleton);
        builder.RegisterComponentOnNewGameObject<MonoTest>("TestSceneObject");
    }
}