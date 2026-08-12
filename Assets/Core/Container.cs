using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavenInject { 
    public enum Lifetime {
        Scoped,
        Singleton
    }

    public struct ImplType {
        public Type Implementation;
        public Lifetime LifeTime;
    }

    public interface IContainerBuilder {
        // Register Methods
        void RegisterEntryPoint<T>();
        void Register<TA, TB>(Lifetime lifeTime);
        void RegisterObjectInScene<TA, TB>(Lifetime lifeTime);
        
        // Resolve Methods
        void ResolveEntryPoints();
        
        // Discard Methods
        void DiscardScopedObjects();
    }

    public class ContainerBuilder : IContainerBuilder {
        private Dictionary<Type, ImplType> _registeredImpls = new Dictionary<Type, ImplType>();
        
        private Dictionary<Type, object> _singletonObjects = new Dictionary<Type, object>();
        private Dictionary<Type, object> _scopedObjects = new Dictionary<Type, object>();
        
        private List<Type> _entryPoints = new List<Type>();
        
        // if (info.Implementation.GetCustomAttributes(typeof(Inject),false).Length > 0) {
        //    foreach (MemberInfo mi in info.Implementation.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
        //        if (mi.MemberType == MemberTypes.Method) {
        //            foreach (ParameterInfo pi in ((MethodInfo) mi).GetParameters()) {
        //                Debug.Log(pi.ParameterType);
        //            }
        //        }
        //    }
        // }
        
        private object Resolve(Type type) {
            // If an object of type is already registered as a Singleton, just hand the same Instance back
            if (_registeredImpls.TryGetValue(type, out var info)) {
                switch (info.LifeTime) {
                    default:
                        return null;
                    
                    case Lifetime.Singleton:
                        if (_singletonObjects.TryGetValue(type, out var singletonObject)) return singletonObject;
                        break;
                    
                    case Lifetime.Scoped:
                        if (_scopedObjects.TryGetValue(type, out var scopedObject)) return scopedObject;
                        break;
                }
                
                // Get the constructor parameters from the Implementation
                Type implementationType = info.Implementation;
                ConstructorInfo constructorInfo = implementationType.GetConstructors()[0];
                ParameterInfo[] constructorParams = constructorInfo.GetParameters();

                // Create a list of all the args and run recursively to get Resolve the parameters Implementation
                List<object> args = new List<object>();
                foreach (ParameterInfo parameterInfo in constructorParams) {
                    Type parameterType = parameterInfo.ParameterType;
                    args.Add(Resolve(parameterType));
                }

                // Create the instance with an array of args
                object instance = constructorInfo.Invoke(args.ToArray());

                // Add the instance to specific cache lists
                switch (info.LifeTime) {
                    default:
                        return null;
                    
                    case Lifetime.Singleton:
                        _singletonObjects.Add(type, instance);
                        break;
                        
                    case Lifetime.Scoped:
                        _scopedObjects.Add(type, instance);
                        break;
                }
                
                return instance;
            }

            foreach (Type registeredEntryPoint in _entryPoints) {
                ConstructorInfo constructorInfo = registeredEntryPoint.GetConstructors()[0];
                ParameterInfo[] constructorParams = constructorInfo.GetParameters();

                List<object> args = new List<object>();
                foreach (ParameterInfo parameterInfo in constructorParams) {
                    Type parameterType = parameterInfo.ParameterType;
                    args.Add(Resolve(parameterType));
                }

                return constructorInfo.Invoke(args.ToArray());
            }

            throw new Exception($"Type {type} is not registered!");
        }

        public void ResolveEntryPoints() {
            foreach (var entryPoint in _entryPoints) {
                Resolve(entryPoint);
            }
        }

        public void RegisterEntryPoint<T>() {
            _entryPoints.Add(typeof(T));
        }
        
        public void Register<TA, TB>(Lifetime lifeTime) {
            ImplType implType = new ImplType {
                Implementation = typeof(TB),
                LifeTime = lifeTime,
            };
            
            _registeredImpls.Add(typeof(TA), implType);

            Debug.unityLogger.Log(LogType.Log, $"Registered Interface: {typeof(TA)} with Class: {typeof(TB)} - and a lifetime of: {lifeTime}");
        }

        public void RegisterObjectInScene<TA, TB>(Lifetime lifeTime) {
            ImplType implType = new ImplType {
                Implementation = typeof(TB),
                LifeTime = lifeTime,
            };
            
            _registeredImpls.Add(typeof(TA), implType);

            Debug.unityLogger.Log(LogType.Log, $"Registered Interface: {typeof(TA)} with MonoBehaviour: {typeof(TB)} - and a LifeTime of: {lifeTime}");
        }

        public void DiscardScopedObjects() {
            _scopedObjects.Clear();
            Debug.unityLogger.Log(LogType.Log, $"Discarded all scoped objects from scene: {SceneManager.GetActiveScene().name}");
        }
    }
}
