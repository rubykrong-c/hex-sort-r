using System;
using System.Collections.Generic;
using Code.Core.Slots.Stack;
using UnityEngine;

namespace Code.Core.LevelDesign
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
        }

        [SerializeField]
        private List<LevelData> _levels = new List<LevelData>();

        public GameObject GetLevelPrefab(int levelId)
        {
            foreach (var entry in _levels)
            {
                if (entry.LevelId == levelId)
                    return entry.LevelPrefab;
            }

            Debug.LogError($"LevelDatabase: уровень {levelId} не найден.");
            return null;
        }

        public int TotalLevels => _levels.Count;
    }

}