using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Core.Slots.Stack
{
    [Serializable]
    public class HexStackData
    {
        [SerializeField] private List<HexStackItem> _items = new();
        public IReadOnlyList<HexStackItem> Items => _items;
        
        public bool IsEmpty => _items.Count == 0;
        
        
        public static HexStackData Create(IEnumerable<HexStackItem> items)
        {
            return new HexStackData(items);
        }
        
        public HexStackData(IEnumerable<HexStackItem> items)
        {
            SetStacks(items);
        }
        
        public void SetStacks(IEnumerable<HexStackItem> items)
        {
            _items.Clear();
            if (items == null) return;

            foreach (var run in items)
            {
                
                _items.Add(run);
            }
        }
    }
    
    [Serializable]
    public struct HexStackItem
    {
        [SerializeField] private EHexType _hexType;   // Цвет
        [SerializeField] private int _count;       // Количество
        
        public EHexType HexType => _hexType;
        public int Count => Mathf.Max(0, _count);
        public bool IsValid => _count > 0;
        
        public HexStackItem(EHexType hexType, int count)
        {
            _hexType = hexType;
            _count = Mathf.Max(0, count);
        }
    }
}