public class InjectorTest {
    private ISceneLoader _sceneLoader;
    private ITest _test;
    
    public InjectorTest(ISceneLoader sceneLoader, ITest test) {
        _sceneLoader = sceneLoader;
        _test = test;
        
        _sceneLoader.Hello();
        _test.Yessir();
    }
}