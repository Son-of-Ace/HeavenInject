using UnityEngine;

namespace HeavenInject {

    public class LifetimeScope : MonoBehaviour {
        private IContainerBuilder _builder;

        protected virtual void Awake() {
            _builder = new ContainerBuilder();
            Bind(_builder);
            _builder.ResolveEntryPoints();
        }

        private void OnDisable() {
            if (!gameObject.scene.isLoaded) {
                _builder.DiscardScopedObjects();
            }
        }

        protected virtual void Bind(IContainerBuilder builder) { }
    }

}