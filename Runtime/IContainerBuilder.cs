using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace HeavenInject {
    public interface IContainerBuilder {
        void Build();
        
        // Register Methods
        void RegisterEntryPoint<T>() where T : class;
        void Register<TA, TB>(LifeTime lifeTime) where TB : class;
        void RegisterSceneObject<T>() where T : Component;
        ObjectRegistration RegisterComponentOnNewGameObject<T>(string objectName = "HIDefaultObject") where T : Component;
        ObjectRegistration RegisterComponentOnNewPrefab<T>(GameObject prefab) where T : Component;
        void AutoRegister(List<GameObject> gameObjects, LifeTime lt);

        // Discard Methods
        void OnScopeDied();
    }
}