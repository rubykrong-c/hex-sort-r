using Code.Core.LevelDesign;
using Code.Core.Slots.Deck;
using UnityEngine;

namespace Code.Core
{
    public class LevelBuilder
    {
        
        private readonly Transform _container;
        private readonly LevelDatabase _levelDatabase;
        private readonly HexDeckHandler _hexDeckHandler;

        public LevelBuilder(
            Transform container, 
            LevelDatabase levelDatabase,
            HexDeckHandler hexDeckHandler)
        {
            _container = container;
            _levelDatabase = levelDatabase;
            _hexDeckHandler = hexDeckHandler;
        }
        
        public void Build(int levelId)
        {
            var levelPrefab = _levelDatabase.GetLevelPrefab(levelId);
            var level = GameObject.Instantiate(levelPrefab, _container);
            
            //_hexDeckHandler.SetDeck();
        }
        
    }
}