using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
        void RegisterComponentOnNewPrefab<T>([CanBeNull] string objectName) where T : Component;
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
        /// <param name="obj"></param>
        /// <param name="lt"></param>
        /// <typeparam name="T"></typeparam>
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
        /// <typeparam name="T"></typeparam>
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
        /// <param name="lifeTime"></param>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
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
        /// <typeparam name="T"></typeparam>
        public void RegisterSceneObject<T>() where T : Component {
            GameObject[] gameObjects = Object.FindObjectsByType<GameObject>();

            foreach (GameObject gameObject in gameObjects) {
                HandleSceneObject<T>(gameObject, LifeTime.Scoped);
            }
        }

        /// <summary>
        /// Takes an Implementation and creates a new GameObject in the scene with that a component of Implementation on it
        /// </summary>
        /// <param name="objectName"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterComponentOnNewGameObject<T>(string objectName = "HeavenInjectDefaultName") where T : Component {
            GameObject newObject = new GameObject(objectName);
            HandleSceneObject<T>(newObject, LifeTime.Scoped);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectName"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterComponentOnNewPrefab<T>(string objectName) where T : Component {
            
        }

        /// <summary>
        /// Automatically registers scene objects sent by LifetimeScope.
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <param name="lt"></param>
        public void AutoRegister(List<GameObject> gameObjects, LifeTime lt) {
            foreach (var gameObject in gameObjects) {
                Component monoComponent = gameObject.GetComponent(typeof(MonoBehaviour));
                Type t = monoComponent.GetType();
            
                ImplementationType implementationType = new ImplementationType() {
                    Implementation = t,
                    LifeTime = lt,
                    SceneObject = monoComponent
                };
            
                _implementations.Add(t, implementationType);
            }
        }

        // When a Scene gets disabled or goes out of scope, this method will run to clear the scoped cache
        public void OnScopeDied() {
            _objectResolver.ClearScopedCache();
        }
    }
}
