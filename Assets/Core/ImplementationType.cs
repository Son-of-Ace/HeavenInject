using System;
using UnityEngine;

namespace HeavenInject {
    public struct ImplementationType {
        public Type Implementation;
        public LifeTime LifeTime;
        public object SceneObject;
    }
}