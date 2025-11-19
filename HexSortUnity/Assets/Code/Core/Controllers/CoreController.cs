using Code.App.Models;
using Code.Core.Level;
using Code.Core.Slots;
using Code.Core.Views;
using Zenject;

namespace Code.Core.Controllers
{
    public class CoreController : IInitializable
    {
        private readonly CoreView _coreView;
        private readonly LevelBuilder _levelBuilder;
        private readonly IUserDataSetter _userData;
        private readonly HexSlotsHandler _hexSlotsHandler;
        
        public CoreController(
            CoreView coreView, 
            LevelBuilder levelBuilder,
            IUserDataSetter userDataSetter,
            HexSlotsHandler hexSlotsHandler)
        {
            _coreView = coreView;
            _levelBuilder = levelBuilder;
            _userData = userDataSetter;
            _hexSlotsHandler = hexSlotsHandler;
        }

        public void Initialize()
        {
            var idLevel = _userData.CurrentLevel;
            
            _levelBuilder.Build(idLevel);
            _hexSlotsHandler.SpawnStacks();
        }
    }
}