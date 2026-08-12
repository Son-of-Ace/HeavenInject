using System;

namespace HeavenInject {
    
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class Inject : Attribute { }

}
