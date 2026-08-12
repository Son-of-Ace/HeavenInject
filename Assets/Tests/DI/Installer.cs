using HeavenInject;

public class Installer : LifetimeScope {
    protected override void Bind(IContainerBuilder builder) {
        builder.RegisterEntryPoint<InjectorTest>();
        builder.RegisterEntryPoint<Errmmm>();
        
        builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
        builder.Register<ITest, Errmmm>(Lifetime.Scoped);
        
        builder.Register<IMonoTest, MonoTest>(Lifetime.Scoped);
    }
}