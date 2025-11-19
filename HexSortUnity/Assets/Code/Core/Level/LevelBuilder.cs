using Code.Core.Slots;
using Code.Core.Slots.Deck;
using UnityEngine;

namespace Code.Core.Level
{
    public class LevelBuilder
    {
        
        private readonly Transform _container;
        private readonly LevelDatabase _levelDatabase;
        private readonly HexDeckHandler _hexDeckHandler;
        private readonly HexSlotsLoader _hexSlotsLoader;

        public LevelBuilder(
            Transform container, 
            LevelDatabase levelDatabase,
            HexDeckHandler hexDeckHandler,
            HexSlotsLoader hexSlotsLoader)
        {
            _container = container;
            _levelDatabase = levelDatabase;
            _hexDeckHandler = hexDeckHandler;
            _hexSlotsLoader = hexSlotsLoader;
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
        }

        private void BuildDeckStacks(int levelId)
        {
            var deck = _levelDatabase.GetDeck(levelId);
            var colors = _levelDatabase.GetAvailableColors(levelId);
            _hexDeckHandler.SetDeck(deck, colors);
        }
    }
}