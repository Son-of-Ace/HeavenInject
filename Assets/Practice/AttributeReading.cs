using System;
using System.Reflection;
using HeavenInject;
using UnityEngine;

namespace Practice {
    public class MyAttrib : Attribute {
        private string _stringVal;
        
        public MyAttrib() {
            _stringVal = "Hello, World from attribute!";
        }
        
        public string StringValue {
            get { return _stringVal; }
            set { _stringVal = value; }
        }
    }
    public class TestClass {
        [Inject]
        public void Construct(ISceneLoader sceneLoader) {
            
        }
        public void Hello(ISceneLoader sceneLoader) {
            
        }
    }
    
    public class AttributeReading : MonoBehaviour {
        private void Awake() {
            Type type = typeof(TestClass);
            MethodInfo[] methods = type.GetMethods();
            
            foreach (MethodInfo method in methods) {
                if (method.IsDefined(typeof(Inject), false)) {
                    ParameterInfo[] pi = method.GetParameters();
                    
                    foreach (ParameterInfo paramInfo in pi) {
                        Debug.Log($"Parameter: {paramInfo.ParameterType}");
                    }
                }
            }
        }
    }
}