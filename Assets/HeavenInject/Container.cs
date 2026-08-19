using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HeavenInject { 
    public enum LifeTime {
        Scoped,
        Singleton
    }

    public interface IContainerBuilder {
        void Build();
        
        // Register Methods
        void RegisterEntryPoint<T>() where T : class;
        void Register<TA, TB>(LifeTime lifeTime) where TB : class;
        void RegisterSceneObject<T>() where T : Component;
        void RegisterComponentOnNewGameObject<T>(string objectName = "HeavenInjectDefaultObject") where T : Component;
        void AutoRegister(List<GameObject> gameObjects, LifeTime lt);

        // Discard Methods
        void OnScopeDied();
    }

    public class ContainerBuilder : IContainerBuilder {
        private IObjectResolver _objectResolver;
        
        // Registered Objects
        private Dictionary<Type, ImplementationType> _implementations = new();

        public void Build() {
            _objectResolver = new ObjectResolver(_implementations);
            
            Initialize();
        }

        // Private Implementation
        // Initialization Methods
        private void Initialize() {
            foreach (var impl in _implementations) {
                _objectResolver.Resolve(impl.Value);
            }
        }
        
        /// <summary>
        /// Takes a GameObject and finds the Inject parameter.
        /// </summary>
        private void HandleSceneObject<T>(GameObject obj, LifeTime lt) {
            Component found = obj.GetComponent(typeof(T));
                
            if (found) {
                ImplementationType implType = new ImplementationType {
                    Implementation = typeof(T),
                    LifeTime = lt,
                    SceneObject = found,
                };
                    
                _implementations.Add(typeof(T), implType);
            }
            else {
                Console.WriteLine($"Object {obj.name} was not found in Scene!");
            }
        }

        // Public Implementation
        // Registration Methods
        /// <summary>
        /// Takes a Class implementation type and registers it in the Container
        /// </summary>
        public void RegisterEntryPoint<T>() where T : class {
            ImplementationType implType = new ImplementationType {
                Implementation = typeof(T),
                LifeTime = LifeTime.Singleton,
                SceneObject = null,
            };
            
            _implementations.Add(typeof(T), implType);

            Debug.unityLogger.Log(LogType.Log, $"Registered EntryPoint: {typeof(T)}");
        }
        
        /// <summary>
        /// Takes an Interface and a Class implementation and registers them in a Dictionary for quick lookup
        /// </summary>
        public void Register<TA, TB>(LifeTime lifeTime) where TB : class {
            ImplementationType implType = new ImplementationType {
                Implementation = typeof(TB),
                LifeTime = lifeTime,
                SceneObject = null,
            };
            
            _implementations.Add(typeof(TA), implType);

            Debug.unityLogger.Log(LogType.Log, $"Registered Interface: {typeof(TA)} with Class: {typeof(TB)} - and a lifetime of: {lifeTime}");
        }
        
        /// <summary>
        /// Takes an Implementation and finds the first object anywhere in the Scene that has a component
        /// with the respected Implementation type on it.
        /// </summary>
        public void RegisterSceneObject<T>() where T : Component {
            GameObject[] gameObjects = Object.FindObjectsByType<GameObject>();

            foreach (GameObject gameObject in gameObjects) {
                HandleSceneObject<T>(gameObject, LifeTime.Scoped);
            }
        }

        /// <summary>
        /// Takes an Implementation and creates a new GameObject in the scene with that a component of Implementation on it
        /// </summary>
        public void RegisterComponentOnNewGameObject<T>(string objectName = "HeavenInjectDefaultName") where T : Component {
            GameObject newObject = new GameObject(objectName);
            HandleSceneObject<T>(newObject, LifeTime.Scoped);
        }

        /// <summary>
        /// Automatically registers scene objects sent by LifetimeScope.
        /// </summary>
        public void AutoRegister(List<GameObject> gameObjects, LifeTime lt) {
            foreach (GameObject gameObject in gameObjects) {
                if (gameObject) {
                    Component[] components = gameObject.GetComponents<Component>();
                }
            }
        }

        // Discard Handling
        public void OnScopeDied() {
            _objectResolver.ClearScopedCache();
        }
    }
}
