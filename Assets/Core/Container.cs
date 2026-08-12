using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenInject { 
    public enum LifeTime {
        Scoped,
        Singleton
    }

    public interface IContainerBuilder {
        void Build();
        
        // Register Methods
        void RegisterEntryPoint<T>();
        void Register<TA, TB>(LifeTime lifeTime);
        void RegisterObjectInScene<TA, TB>(LifeTime lifeTime);
        
        // Initialize Methods
        void InitializeEntryPoints();
        
        // Discard Methods
        void OnScopeDied();
    }

    public class ContainerBuilder : IContainerBuilder {
        private IObjectResolver _objectResolver;
        
        // Registered Objects
        private Dictionary<Type, ImplementationType> _registeredImpls = new();
        
        // Single Lists
        private List<Type> _entryPoints = new();

        public void Build() {
            _objectResolver = new ObjectResolver(_registeredImpls);
        }

        // Initialization Methods
        public void InitializeEntryPoints() {
            foreach (var entryPoint in _entryPoints) {
                _objectResolver.ResolveEntryPoint(entryPoint);
            }
        }

        // Registration Methods
        public void RegisterEntryPoint<T>() {
            _entryPoints.Add(typeof(T));
        }
        
        public void Register<TA, TB>(LifeTime lifeTime) {
            ImplementationType implType = new ImplementationType {
                Implementation = typeof(TB),
                LifeTime = lifeTime,
            };
            
            _registeredImpls.Add(typeof(TA), implType);

            Debug.unityLogger.Log(LogType.Log, $"Registered Interface: {typeof(TA)} with Class: {typeof(TB)} - and a lifetime of: {lifeTime}");
        }

        public void RegisterObjectInScene<TA, TB>(LifeTime lifeTime) {
            ImplementationType implType = new ImplementationType {
                Implementation = typeof(TB),
                LifeTime = lifeTime,
            };
            
            _registeredImpls.Add(typeof(TA), implType);

            Debug.unityLogger.Log(LogType.Log, $"Registered Interface: {typeof(TA)} with MonoBehaviour: {typeof(TB)} - and a LifeTime of: {lifeTime}");
        }

        // Discard Handling
        public void OnScopeDied() {
            _objectResolver.ClearScopedCache();
        }
    }
}
