using System;
using System.Collections.Generic;
using System.Reflection;

namespace HeavenInject {
    public interface IObjectResolver {
        object Resolve(ImplementationType implType);

        void ClearScopedCache();
    }
    
    public class ObjectResolver : IObjectResolver {
        private Dictionary<Type, ImplementationType> _implementations;
        
        // Circular Dependency Cache
        private List<Type> _resolutionPath = new();
        
        // Cached Objects
        private Dictionary<Type, object> _singletonCache = new();
        private Dictionary<Type, object> _scopedCache = new();

        public ObjectResolver(Dictionary<Type, ImplementationType> implementations) {
            _implementations = implementations;
        }
        
        // Internal API
        /// <summary>
        /// Takes in an Implementation type and searches for a constructor or a method with an Inject attribute.
        /// When either is found, it will Invoke the constructor or method found with the resolved arguments.
        /// </summary>
        private object ConstructInstance(ImplementationType implType) {
            List<object> args = new List<object>();

            Type implementation = implType.Implementation;

            if (implType.SceneObject != null) {
                // BindingFlags for GetMethods call to let it know what Methods it is allowed to inspect
                BindingFlags flags =
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance;
                
                // Gets all methods from the Implementation type and performs a loop on them
                MethodInfo[] methods = implementation.GetMethods(flags);
                foreach (MethodInfo method in methods) {
                    // Check if the Method has an Attribute of type Inject
                    if (!method.IsDefined(typeof(Inject), false)) continue;
                    
                    // Get all parameters out of the found Method
                    ParameterInfo[] pi = method.GetParameters();
                
                    // Loop all parameters and add their resolved type to the Args list
                    foreach (ParameterInfo paramInfo in pi) {
                        Type parameterType = paramInfo.ParameterType;
                        args.Add(Resolve(parameterType));
                    }
                    
                    // Invokes the Method on the Component found in the scene from RegisterSceneObject with the Resolved args
                    method.Invoke(implType.SceneObject, args.ToArray());
                    return implType.SceneObject;
                }
            }
            else {
                // Get the constructor parameters from the Implementation
                ConstructorInfo constructorInfo = implementation.GetConstructors()[0];
                ParameterInfo[] constructorParams = constructorInfo.GetParameters();
                
                // Create a list of all the ParameterTypes in the constructor,
                // and run recursively to Resolve the Interface implementations of the parameters in the constructor
                foreach (ParameterInfo parameterInfo in constructorParams) {
                    Type parameterType = parameterInfo.ParameterType;
                    args.Add(Resolve(parameterType));
                }
            
                // Invoke the Constructor with the resolved args
                return constructorInfo.Invoke(args.ToArray());
            }

            // If no Implementation type was registered, throw an exception.
            throw new Exception($"Type: {implementation} does not define a Constructor or a Method with the [Inject] attribute!");
        }

        /// <summary>
        /// Takes in a Type and finds the ImplementationType associated with that Type.
        /// When a valid ImplementationType is found, it will return a cached instance if found or construct a new one and cache it.
        /// Circular dependency is also handled with a nice Exception if it happens.
        /// </summary>
        private object Resolve(Type type) {
            if (!_implementations.TryGetValue(type, out ImplementationType impl)) {
                throw new Exception($"Type: {type} has not been registered!");
            }
            
            if (_resolutionPath.Contains(type)) {
                string path = string.Join(
                    " -> ",
                    _resolutionPath
                );
                
                throw new Exception($"Circular dependency detected: {path} -> {type}");
            }
            
            _resolutionPath.Add(type);

            try {
                Dictionary<Type, object> cache = impl.LifeTime switch {
                    LifeTime.Singleton => _singletonCache,
                    LifeTime.Scoped => _scopedCache,
                    _ => null
                };
                
                Type implementation = impl.Implementation;

                if (cache != null && cache.TryGetValue(implementation, out object existingObject)) {
                    return existingObject;
                }

                object instance = ConstructInstance(impl);

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
        public object Resolve(ImplementationType implType) {
            return ConstructInstance(implType);
        }

        /// <summary>
        /// When a scene gets unloaded, all the objects registered with a Scoped lifetime within that scenes LifetimeScope will be cleared.
        /// </summary>
        public void ClearScopedCache() {
            _scopedCache.Clear();
        }
    }
}