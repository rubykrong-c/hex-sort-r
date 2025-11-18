using System;
using System.Collections;
using Code.App.Managers.LoadingBar;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Code.App.Managers
{
    public class SceneLoader 
    {
        private const string LOADING_SCENE_NAME = "LoadingScene";
        private const string ROOT_SCENE_NAME = "RootScene";
        private const string CORE_SCENE_NAME = "CoreScene";

        [Inject] private LoadingBarAdapter _loadingBarAdapter;


        public void LoadScene(EScene scene, Action callback = null)
        {
            var nameScene = GetNameScene(scene);
            
            LoadScenes(nameScene, callback).Forget();
        }

        private string GetNameScene(EScene scene)
        {
            switch (scene)
            {
                case EScene.ROOT:
                    return ROOT_SCENE_NAME;
                    break;
                case EScene.CORE:
                    return CORE_SCENE_NAME;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scene), scene, null);
            }
        }

        
        private async UniTask LoadScenes(string sceneName, Action sceneLoaded = null)
        {
            // Load LoadingScene additively and asynchronously.
            var loadingSceneAsync = SceneManager.LoadSceneAsync(LOADING_SCENE_NAME, LoadSceneMode.Additive);
            await loadingSceneAsync.ToUniTask();

            // Start loading bar animation (start)
            await _loadingBarAdapter.LoadingBarKeeper
                .RunDependentLoadingBarAnimation(DependLoadingType.GAME_SCENE_LOAD_START);

            // Unload Root Scene
            var unloadRootSceneAsync = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            // Load target scene additively and asynchronously.
            var mainMenuSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            await unloadRootSceneAsync.ToUniTask();
            await mainMenuSceneAsync.ToUniTask();

            // Set target scene as active
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            // Finish loading bar animation (end)
            await _loadingBarAdapter.LoadingBarKeeper
                .RunDependentLoadingBarAnimation(DependLoadingType.GAME_SCENE_LOAD_END);

            // Unload Loading Scene
            var unloadLoadingSceneAsync = SceneManager.UnloadSceneAsync(LOADING_SCENE_NAME);
            await unloadLoadingSceneAsync.ToUniTask();

            // Callback
            sceneLoaded?.Invoke();
        }

    }

    public enum EScene
    {
        ROOT,
        CORE
    }
}