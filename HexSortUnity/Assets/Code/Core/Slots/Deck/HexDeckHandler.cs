using System;
using System.Collections.Generic;
using Code.Core.Slots.Stack;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using Random = System.Random;

namespace Code.Core.Slots.Deck
{
     public class HexDeckHandler
    {
        private readonly PoolingSystem _poolingSystem;
        private readonly Settings _settings;
        private readonly Queue<HexStackData> _stackDefinitions = new();
        private readonly List<EHexType> _availableColors = new();

        public HexDeckHandler(PoolingSystem poolingSystem, Settings settings)
        {
            _poolingSystem = poolingSystem;
            _settings = settings;
        }

        public void SetDeck(IEnumerable<HexStackData> definitions, IEnumerable<EHexType> levelColors)
        {
            _stackDefinitions.Clear();
            if (definitions != null)
            {
                foreach (var definition in definitions)
                {
                    if (definition == null || definition.IsEmpty)
                    {
                        continue;
                    }
                    _stackDefinitions.Enqueue(definition);
                }
            }

            _availableColors.Clear();
            if (levelColors != null)
            {
                foreach (var colorId in levelColors)
                {
                    if (!_availableColors.Contains(colorId))
                    {
                        _availableColors.Add(colorId);
                    }
                }
            }
        }

        public HexStack TryGetNextStack(bool enableRandomFallback)
        {
            if (_stackDefinitions.Count > 0)
            {
                var definition = _stackDefinitions.Dequeue();
                return CreateStack(definition);
            }

            if (!enableRandomFallback)
            {
                return null;
            }

            var randomDefinition = CreateRandomDefinition();
            return CreateStack(randomDefinition);
        }

        private HexStack CreateStack(HexStackData definition)
        {
            if (definition == null || definition.IsEmpty)
            {
                return null;
            }

            HexStack stack = _poolingSystem.InstantiateAPS(_settings.StackPoolKey).GetComponent<HexStack>();
            List<HexElement> createdElements = new List<HexElement>();
            foreach (var run in definition.Items)
            {
                if (!run.IsValid) continue;
                Material material = null;
                _settings.ColorMaterials?.TryGetValue(run.HexType, out material);
                for (int i = 0; i < run.Count; i++)
                {
                    HexElement element = _poolingSystem.InstantiateAPS(_settings.ElementPoolKey).GetComponent<HexElement>();
                    element.Configure(run.HexType, material);
                    createdElements.Add(element);
                }
            }

            stack.SetStack(createdElements, definition.Items, _poolingSystem);
            return stack;
        }

        private HexStackData CreateRandomDefinition()
        {
            RandomGenerationSettings randomSettings = _settings.RandomSettings;

            int minHeight = Mathf.Max(1, randomSettings.StackHeightRange.x);
            int maxHeight = Mathf.Max(minHeight, randomSettings.StackHeightRange.y);
            int stackHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
            int remaining = stackHeight;

            List<EHexType> colorPool = GetRuntimeColorPool();
            if (colorPool.Count == 0)
                throw new InvalidOperationException("HexDeckHandler: no colors available for random generation.");

            List<HexStackItem> hexStackItems = new();
            EHexType lastColor = default;
            bool hasLastColor = false;

            while (remaining > 0)
            {
                EHexType color = PickNextColor(colorPool, hasLastColor ? lastColor : default);

                int minRun = Mathf.Max(1, randomSettings.RunLengthRange.x);
                int maxRun = Mathf.Max(minRun, randomSettings.RunLengthRange.y);
                int desiredLength = UnityEngine.Random.Range(minRun, maxRun + 1);

                int length = Mathf.Min(desiredLength, remaining);

                hexStackItems.Add(new HexStackItem(color, length));

                remaining -= length;
                lastColor = color;
                hasLastColor = true;
            }

            return HexStackData.Create(hexStackItems);
        }

        private EHexType PickNextColor(List<EHexType> colors, EHexType lastColor)
        {
            if (colors == null || colors.Count == 0)
            {
                return lastColor;
            }
            
            int index = UnityEngine.Random.Range(0, colors.Count);
            EHexType picked = colors[index];

            // если совпало с предыдущим цветом — берём следующий в списке
            if (picked.Equals(lastColor) && colors.Count > 1)
            {
                int nextIndex = (index + 1) % colors.Count;
                picked = colors[nextIndex];
            }

            return picked;
        }

        private List<EHexType> GetRuntimeColorPool()
        {
            // 1. Если есть динамически доступные цвета — использовать их
            if (_availableColors.Count > 0)
            {
                return _availableColors;
            }

            // 2. Если задан дефолтный пул цветов в настройках — использовать его
            if (_settings.RandomSettings.DefaultColors != null &&
                _settings.RandomSettings.DefaultColors.Count > 0)
            {
                return _settings.RandomSettings.DefaultColors;
            }

            // 3. Пустой пул, чтобы не словить null
            return new List<EHexType>();
        }

        [Serializable]
        public class Settings
        {
            public string StackPoolKey = "HexStack";
            public string ElementPoolKey = "HexElement";
            public HexColorMaterialDictionary ColorMaterials;
            public RandomGenerationSettings RandomSettings = new();
        }

        [Serializable]
        public class HexColorMaterialDictionary : SerializableDictionaryBase<EHexType, Material>
        {
        }

        [Serializable]
        public class RandomGenerationSettings
        {
            public List<EHexType> DefaultColors;
            public Vector2Int StackHeightRange;
            public Vector2Int RunLengthRange;
        }
    }
}