using Code.Core.LevelDesign;
using UnityEngine;

namespace Code.Core
{
    public class LevelBuilder
    {
        
        private readonly Transform _container;
        private readonly LevelDatabase _levelDatabase;

        public LevelBuilder(Transform container, LevelDatabase levelDatabase)
        {
            _container = container;
            _levelDatabase = levelDatabase;
        }
        
        public void Build(int levelId)
        {
            var levelPrefab = _levelDatabase.GetLevelPrefab(levelId);
            var level = GameObject.Instantiate(levelPrefab, _container);
        }
        
    }
}