using System;
using System.Collections.Generic;
using Code.Core.Slots.Stack;
using UnityEngine;

namespace Code.Core.Level
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [Serializable]
        public class LevelData
        {
            public int LevelId;                
            public GameObject LevelPrefab;    
            public List<HexStackData> Stacks;
            public List<EHexType> AvailableColor;
        }

        [SerializeField]
        private List<LevelData> _levels = new List<LevelData>();

        public GameObject GetLevelPrefab(int levelId)
        {
            return GetLevelData(levelId)?.LevelPrefab;
        }

        public IEnumerable<HexStackData> GetDeck(int levelId)
        {
          return GetLevelData(levelId)?.Stacks;
        }

        public List<EHexType> GetAvailableColors(int levelId)
        {
            return GetLevelData(levelId)?.AvailableColor;
        }
        
        public int TotalLevels => _levels.Count;

        private LevelData GetLevelData(int levelId)
        {
            var data = _levels.Find(x => x.LevelId == levelId);

            if (data != null)
            {
                return data;
            }

            Debug.LogError($"LevelDatabase: уровень {levelId} не найден.");
            return null;
        }
    }

}