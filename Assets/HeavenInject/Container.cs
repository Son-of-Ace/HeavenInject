using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HeavenInject { 
    public enum LifeTime {
        Scoped,
        Singleton
    }

    public class ContainerBuilder : IContainerBuilder {
        private IObjectResolver _objectResolver;
        
        // Registered Objects
        private Dictionary<Type, ImplementationType> _implementations = new();
        
        /// <summary>
        /// Variables used to store objects for UnderTransform method to work.
        /// </summary>
        private GameObject _newObject;

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
        /// Takes an Implementation and creates a new GameObject in the scene with a component of the Implementation on it
        /// </summary>
        /// <param name="objectName"></param>
        /// <typeparam name="T"></typeparam>
        public ContainerBuilder RegisterComponentOnNewGameObject<T>(string objectName = "HIDefaultObjectName") where T : Component {
            _newObject = new GameObject(objectName);
            _newObject.AddComponent<T>();
            HandleSceneObject<T>(_newObject, LifeTime.Scoped);
            return this;
        }

        /// <summary>
        /// Takes an Implementation and creates a new Prefab in the scene and project with a component of the Implementation on it 
        /// </summary>
        /// <param name="objectName"></param>
        /// <typeparam name="T"></typeparam>
        public ContainerBuilder RegisterComponentOnNewPrefab<T>(string objectName = "HIDefaultPrefabName") where T : Component {
            return this;
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
        
        /// <summary>
        /// Chain Methods
        /// </summary>
        public ContainerBuilder UnderTransform(Transform parent) {
            _newObject.transform.parent = parent;
            return this;
        }
        
        /// <summary>
        /// When a Scene gets disabled or goes out of scope, this method will run to clear the scoped cache
        /// </summary>
        public void OnScopeDied() {
            _objectResolver.ClearScopedCache();
        }
    }
}
