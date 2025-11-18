using Code.App.Managers;
using Zenject;

namespace Code.App
{
    public class AppController: IInitializable
    {
        private SceneLoader _sceneLoader;

        public AppController(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }
        
        public void Initialize()
        {
            _sceneLoader.LoadScene(EScene.CORE);
        }
    }
}