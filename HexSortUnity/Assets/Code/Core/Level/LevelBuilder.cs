using Code.Core.Slots;
using Code.Core.Slots.Deck;
using UnityEngine;
using Code.Core;

namespace Code.Core.Level
{
    public class LevelBuilder
    {
        
        private readonly Transform _container;
        private readonly LevelDatabase _levelDatabase;
        private readonly HexDeckHandler _hexDeckHandler;
        private readonly HexSlotsLoader _hexSlotsLoader;
        private readonly FieldModel _fieldModel;

        public LevelBuilder(
            Transform container, 
            LevelDatabase levelDatabase,
            HexDeckHandler hexDeckHandler,
            HexSlotsLoader hexSlotsLoader,
            FieldModel fieldModel)
        {
            _container = container;
            _levelDatabase = levelDatabase;
            _hexDeckHandler = hexDeckHandler;
            _hexSlotsLoader = hexSlotsLoader;
            _fieldModel = fieldModel;
        }
        
        public void Build(int levelId)
        {
            _hexSlotsLoader.LoadSlots();
            BuildField(levelId);
            BuildDeckStacks(levelId);
        }

        private void BuildField(int levelId)
        {
            var levelPrefab = _levelDatabase.GetLevelPrefab(levelId);
            var level = GameObject.Instantiate(levelPrefab, _container);
            //TODO: Сделать View уровня и оттуда ссылки тягать
            var tiles = level.GetComponentsInChildren<TileView>(true);
            _fieldModel.Build(tiles);
        }

        private void BuildDeckStacks(int levelId)
        {
            var deck = _levelDatabase.GetDeck(levelId);
            var colors = _levelDatabase.GetAvailableColors(levelId);
            _hexDeckHandler.SetDeck(deck, colors);
        }
    }
}
