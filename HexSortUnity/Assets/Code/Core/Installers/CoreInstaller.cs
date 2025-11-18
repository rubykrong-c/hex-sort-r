using Code.Core.Controllers;
using Code.Core.LevelDesign;
using Code.Core.Views;
using UnityEngine;
using Zenject;

namespace Code.Core.Installers
{
    public class CoreInstaller : MonoInstaller
    {
        [SerializeField] private CoreView _coreView;
        [SerializeField] private LevelDatabase _levelDatabase;

        public override void InstallBindings()
        {
            Container.Bind(typeof(IInitializable))
                .To<CoreController>()
                .AsSingle();
            
            Container.Bind<CoreView>()
                .FromInstance(_coreView)
                .WhenInjectedInto<CoreController>();

            Container.Bind<LevelBuilder>().AsSingle();
            
            Container.Bind<LevelDatabase>()
                .FromInstance(_levelDatabase)
                .WhenInjectedInto<LevelBuilder>(); 
            
            Container.Bind<Transform>()
                .FromInstance(_coreView.LevelRoot)
                .WhenInjectedInto<LevelBuilder>();

            
        }
    }
}