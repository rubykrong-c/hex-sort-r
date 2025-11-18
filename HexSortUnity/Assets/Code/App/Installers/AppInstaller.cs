using UnityEngine;
using Zenject;
using Code.App.Managers;
using Code.App.Managers.LoadingBar;
using Code.App.Models;

namespace Code.App.Installers
{
    public class AppInstaller : MonoInstaller 
    {
  
        public override void InstallBindings()
        {
            Container.Bind<SceneLoader>().AsSingle().NonLazy();
            Container.Bind(typeof(IInitializable))
                .To<AppController>()
                .AsSingle();
            
            Container.Bind<LoadingBarAdapter>().AsSingle();

            BindModels();
        }

        private void BindModels()
        {
            Container
                .Bind(typeof(IUserDataGetter), typeof(IUserDataSetter))
                .To<UserDataModel>()
                .AsSingle();
        }
    }
}
