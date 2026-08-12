using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
        void Register<TA, TB>(Lifetime lifetime);
        
        // Resolve Methods
        void ResolveEntryPoints();
        
        // Discard Methods
        void DiscardScopedObjects();
    }

    public class ContainerBuilder : IContainerBuilder {
        private Dictionary<Type, ImplType> _implementationTypes = new Dictionary<Type, ImplType>();
        
        private Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private Dictionary<Type, object> _scoped = new Dictionary<Type, object>();
        
        private List<Type> _entryPoints = new List<Type>();
        
        private object Resolve(Type type) {
            // If an object of type is already registered as a Singleton, just hand the same Instance back
            if (_implementationTypes.TryGetValue(type, out var info)) {
                switch (info.LifeTime) {
                    default:
                        return null;
                    
                    case Lifetime.Singleton:
                        if (_singletons.TryGetValue(type, out var singleton)) return singleton;
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
                        _singletons.Add(type, instance);
                        break;
                        
                    case Lifetime.Scoped:
                        _scoped.Add(type, instance);
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
        
        public void Register<TA, TB>(Lifetime lifetime) {
            ImplType implType = new ImplType {
                Implementation = typeof(TB),
                LifeTime = lifetime
            };
            
            _implementationTypes.Add(typeof(TA), implType);

            Debug.unityLogger.Log(LogType.Log, $"Registered interface: {typeof(TA)} with class: {typeof(TB)} - and a lifetime of: {lifetime}");
        }

        public void DiscardScopedObjects() {
            _scoped.Clear();
            Debug.unityLogger.Log(LogType.Log, $"Discarded all scoped objects");
        }
    }
}
