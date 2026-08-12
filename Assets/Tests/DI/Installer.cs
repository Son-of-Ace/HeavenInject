using HeavenInject;

public class Installer : LifetimeScope {
    protected override void Bind(IContainerBuilder builder) {
        builder.RegisterEntryPoint<InjectorTest>();
        
        builder.Register<ISceneLoader, SceneLoader>(LifeTime.Singleton);
        builder.Register<ITest, Errmmm>(LifeTime.Scoped);
    }
}