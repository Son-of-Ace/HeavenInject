using HeavenInject;
using UnityEngine;

public class MonoTest : MonoBehaviour {
    [Inject]
    private void Construct(ISceneLoader sceneLoader) {
        sceneLoader.Hello();
    }
}
