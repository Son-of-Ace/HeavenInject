using UnityEngine;

public class Errmmm : ITest {
    public Errmmm(ISceneLoader sceneLoader) {
        sceneLoader.Hello();
    }
    
    public void Yessir() {
        Debug.Log("Errrrm, yep!");
    }
}