using HeavenInject;
using UnityEngine;

public class MonoTest : MonoBehaviour, IMonoTest {
    [Inject]
    private void Construct(ISceneLoader sceneLoader) {
        sceneLoader.Hello();
    }
}
