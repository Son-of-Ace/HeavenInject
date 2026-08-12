using System;
using System.Reflection;
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
    
    [MyAttrib(StringValue = "I changed this for a Class")]
    public class AttributeReading : MonoBehaviour {
        private MemberInfo info = typeof(AttributeReading);

        private void Awake() {
            foreach (object attrib in info.GetCustomAttributes(true)) {
                Debug.Log(attrib);
            }
        }
    }
}