using System.Collections.Generic;
using UnityEngine;

namespace HeavenInject {
    public class LifetimeScope : MonoBehaviour {
        [Header("Settings")]
        [Tooltip("Tells the scope what Lifetime to register AutoRegisterObjects with")]
        [SerializeField] private LifeTime lt;

        [Tooltip("Tells the scope what objects to automatically register")]
        [SerializeField] private List<GameObject> autoRegisterObjects;
        
        private IContainerBuilder _builder;

        protected virtual void Awake() {
            _builder = new ContainerBuilder();
            Bind(_builder);
            _builder.AutoRegister(autoRegisterObjects, lt);
            _builder.Build();
        }

        private void OnDisable() {
            if (!gameObject.scene.isLoaded) {
                _builder.OnScopeDied();
            }
        }

        protected virtual void Bind(IContainerBuilder builder) { }
    }
}