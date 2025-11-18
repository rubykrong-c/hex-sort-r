using Code.App.Models;
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
        private readonly HexSlotsLoader _hexSlotsLoader;
        
        public CoreController(
            CoreView coreView, 
            LevelBuilder levelBuilder,
            IUserDataSetter userDataSetter,
            HexSlotsLoader hexSlotsLoader)
        {
            _coreView = coreView;
            _levelBuilder = levelBuilder;
            _userData = userDataSetter;
            _hexSlotsLoader = hexSlotsLoader;
        }

        public void Initialize()
        {
            var idLevel = _userData.CurrentLevel;
            
            _hexSlotsLoader.LoadSlots();
            _levelBuilder.Build(idLevel);
        }
    }
}