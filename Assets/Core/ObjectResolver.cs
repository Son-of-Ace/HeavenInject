using System;
using System.Collections.Generic;
using System.Reflection;

// INJECT ATTRIBUTE HANDLING CODE
// if (info.Implementation.GetCustomAttributes(typeof(Inject),false).Length > 0) {
//    foreach (MemberInfo mi in info.Implementation.GetMembers(BindingFlags.NonPublic)) {
//        if (mi.MemberType == MemberTypes.Method) {
//            foreach (ParameterInfo pi in ((MethodInfo) mi).GetParameters()) {
//                Debug.Log(pi.ParameterType);
//            }
//        }
//    }
// }

namespace HeavenInject {
    public interface IObjectResolver {
        object ResolveEntryPoint(Type entrypoint);
        object ResolveAttribute(Type attribType);

        void ClearScopedCache();
    }
    
    public class ObjectResolver : IObjectResolver {
        private Dictionary<Type, ImplementationType> _registeredImpls;
        private List<Type> _resolutionPath = new();
        
        // Cached Objects
        private Dictionary<Type, object> _singletonCache = new();
        private Dictionary<Type, object> _scopedCache = new();

        // Constructor
        public ObjectResolver(Dictionary<Type, ImplementationType> registeredImpls) {
            _registeredImpls = registeredImpls;
        }
        
        // Internal API
        private object ConstructInstance(Type type) {
            // Get the constructor parameters from the Implementation
            ConstructorInfo constructorInfo = type.GetConstructors()[0];
            ParameterInfo[] constructorParams = constructorInfo.GetParameters();

            // Create a list of all the args and run recursively to Resolve the parameters Implementation
            List<object> args = new List<object>();
            foreach (ParameterInfo parameterInfo in constructorParams) {
                Type parameterType = parameterInfo.ParameterType;
                args.Add(Resolve(parameterType));
            }

            // Create the instance with an array of args
            return constructorInfo.Invoke(args.ToArray());
        }

        private object Resolve(Type type) {
            if (!_registeredImpls.TryGetValue(type, out ImplementationType implType))
                throw new Exception($"Type {type} is not registered!");

            if (_resolutionPath.Contains(type)) {
                string path = string.Join(
                    " -> ",
                    _resolutionPath
                );
                
                throw new Exception($"Circular dependency detected: {path} -> {type}");
            }
            
            _resolutionPath.Add(type);

            try {
                Type implementation = implType.Implementation;

                Dictionary<Type, object> cache = implType.LifeTime switch {
                    LifeTime.Singleton => _singletonCache,
                    LifeTime.Scoped => _scopedCache,
                    _ => null
                };

                if (cache != null && cache.TryGetValue(implementation, out object existingObject)) {
                    return existingObject;
                }

                object instance = ConstructInstance(implType.Implementation);

                if (cache != null) {
                    cache.Add(implementation, instance);
                }

                return instance;
            }
            finally {
                _resolutionPath.RemoveAt(_resolutionPath.Count - 1);
            }
        }
        
        // Public API
        public object ResolveEntryPoint(Type entryPoint) {
            return ConstructInstance(entryPoint);
        }
        
        public object ResolveAttribute(Type attribType) {
            return null;
        }

        public void ClearScopedCache() {
            _scopedCache.Clear();
        }
    }
}