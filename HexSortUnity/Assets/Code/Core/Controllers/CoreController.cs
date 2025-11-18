using Code.App.Models;
using Code.Core.Views;
using Zenject;

namespace Code.Core.Controllers
{
    public class CoreController : IInitializable
    {
        private readonly CoreView _coreView;
        private readonly LevelBuilder _levelBuilder;
        private readonly IUserDataSetter _userData;
        
        public CoreController(
            CoreView coreView, 
            LevelBuilder levelBuilder,
            IUserDataSetter userDataSetter)
        {
            _coreView = coreView;
            _levelBuilder = levelBuilder;
            _userData = userDataSetter;
        }

        public void Initialize()
        {
            var idLevel = _userData.CurrentLevel;
            _levelBuilder.Build(idLevel);
        }
    }
}