using UnityEngine;

namespace HeavenInject {
    public readonly struct ObjectRegistration {
        private readonly GameObject _gameObject;
        private readonly ContainerBuilder _builder;

        internal ObjectRegistration(GameObject gameObject, ContainerBuilder builder) {
            _gameObject = gameObject;
            _builder = builder;
        }

        public ContainerBuilder UnderTransform(Transform parent) {
            if (!parent) {
                Debug.Log($"[ERROR]: Parent not found in scene or on the scene installer!");
                return null;
            }
        
            _gameObject.transform.parent = parent;
            return _builder;
        }
    }
}