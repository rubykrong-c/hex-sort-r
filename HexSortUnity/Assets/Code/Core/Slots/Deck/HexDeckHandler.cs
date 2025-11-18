using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace Code.Core.Slots.Deck
{
     public class HexDeckHandler
    {
        private readonly PoolingSystem _poolingSystem;
        private readonly Settings _settings;
        private readonly Queue<HexStackDefinition> _stackDefinitions = new();
        private readonly List<int> _availableColors = new();

        public HexDeckHandler(PoolingSystem poolingSystem, Settings settings)
        {
            _poolingSystem = poolingSystem;
            _settings = settings;
        }

        public void SetDeck(IEnumerable<HexStackDefinition> definitions, IEnumerable<int> levelColors)
        {
            _stackDefinitions.Clear();
            if (definitions != null)
            {
                foreach (var definition in definitions)
                {
                    if (definition == null || definition.IsEmpty) continue;
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

        private HexStack CreateStack(HexStackDefinition definition)
        {
            if (definition == null || definition.IsEmpty)
            {
                return null;
            }

            HexStack stack = _poolingSystem.InstantiateAPS(_settings.StackPoolKey).GetComponent<HexStack>();
            List<HexElement> createdElements = new List<HexElement>();
            foreach (var run in definition.ColorRuns)
            {
                if (!run.IsValid) continue;
                Material material = null;
                _settings.ColorMaterials?.TryGetValue(run.ColorId, out material);
                for (int i = 0; i < run.Count; i++)
                {
                    HexElement element = _poolingSystem.InstantiateAPS(_settings.ElementPoolKey).GetComponent<HexElement>();
                    element.Configure(run.ColorId, material);
                    createdElements.Add(element);
                }
            }

            stack.SetStack(createdElements, definition.ColorRuns, _poolingSystem);
            return stack;
        }

        private HexStackDefinition CreateRandomDefinition()
        {
            RandomGenerationSettings randomSettings = _settings.RandomSettings;
            int minHeight = Mathf.Max(1, randomSettings.StackHeightRange.x);
            int maxHeight = Mathf.Max(minHeight, randomSettings.StackHeightRange.y);
            int stackHeight = UnityEngine.Random.Range(minHeight, maxHeight + 1);
            int remaining = stackHeight;

            List<int> colorPool = GetRuntimeColorPool();
            if (colorPool.Count == 0)
            {
                throw new InvalidOperationException("HexDeckHandler: no colors available for random generation.");
            }

            List<HexColorRun> runs = new List<HexColorRun>();
            int lastColor = -1;
            while (remaining > 0)
            {
                int colorId = PickNextColor(colorPool, lastColor);
                int minRun = Mathf.Max(1, randomSettings.RunLengthRange.x);
                int maxRun = Mathf.Max(minRun, randomSettings.RunLengthRange.y);
                int desiredLength = UnityEngine.Random.Range(minRun, maxRun + 1);
                int length = Mathf.Min(desiredLength, remaining);
                runs.Add(new HexColorRun(colorId, length));
                remaining -= length;
                lastColor = colorId;
            }

            return HexStackDefinition.Create(runs);
        }

        private int PickNextColor(List<int> colors, int lastColor)
        {
            if (colors.Count == 0)
            {
                return lastColor;
            }

            int colorId = colors[UnityEngine.Random.Range(0, colors.Count)];
            if (colorId == lastColor && colors.Count > 1)
            {
                colorId = colors[(colors.IndexOf(colorId) + 1) % colors.Count];
            }
            return colorId;
        }

        private List<int> GetRuntimeColorPool()
        {
            if (_availableColors.Count > 0)
            {
                return _availableColors;
            }

            if (_settings.RandomSettings.DefaultColorIds != null && _settings.RandomSettings.DefaultColorIds.Count > 0)
            {
                return _settings.RandomSettings.DefaultColorIds;
            }

            return new List<int>();
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
        public class HexColorMaterialDictionary : SerializableDictionaryBase<int, Material>
        {
        }

        [Serializable]
        public class RandomGenerationSettings
        {
            public Vector2Int StackHeightRange = new Vector2Int(3, 8);
            public Vector2Int RunLengthRange = new Vector2Int(1, 4);
            public List<int> DefaultColorIds = new List<int> { 0, 1, 2 };
        }
    }
}