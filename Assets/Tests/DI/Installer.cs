using HeavenInject;
using UnityEngine;

public class Installer : LifetimeScope {
    [SerializeField] private Transform _parent;
    
    protected override void Bind(IContainerBuilder builder) {
        builder.Register<ISceneLoader, SceneLoader>(LifeTime.Singleton);
        builder.RegisterComponentOnNewGameObject<MonoTest>("TestSceneObject").UnderTransform(_parent);
        //builder.RegisterComponentOnNewPrefab<MonoTest>("NewPrefab").UnderTransform(parent);
    }
}