using System.Collections.Generic;
using Code.Core.Slots;
using Code.Core.Slots.Stack;
using UnityEngine;

namespace Code
{
    public class HexStack : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private Transform _elementsRoot;

        [SerializeField]
        private float _elementHeightStep = 0.05f;
        

        [SerializeField]
        private bool _enableElementColliders = true;

        private readonly List<HexElement> _elements = new();
        private readonly List<HexStackItemInfo> _hexStackItemsInfo = new();

        private PoolingSystem _poolingSystem;
        private HexSlotView _currentSlot;
        private HexFieldChunk _currentChunk;

        public Transform ElementsRoot => _elementsRoot;
        public IReadOnlyList<HexElement> Elements => _elements;
        public IReadOnlyList<HexStackItemInfo> HexStackItemsInfo => _hexStackItemsInfo;
        public bool IsPlacedOnField => _currentChunk != null;

        public void SetStack(IReadOnlyList<HexElement> orderedElements,
                              IReadOnlyList<HexStackItem> orderedHexStackItems,
                              PoolingSystem poolingSystem)
        {
            _poolingSystem = poolingSystem;
            ReleaseElementsToPool();

            if (orderedElements != null)
            {
                for (int i = 0; i < orderedElements.Count; i++)
                {
                    HexElement element = orderedElements[i];
                    if (element == null) continue;
                    _elements.Add(element);
                    element.AttachToStack(this, i);
                    element.ToggleCollider(_enableElementColliders);
                }
            }

            ApplyRuns(orderedHexStackItems);
            UpdateElementLayout();
        }

        public void AttachToSlot(HexSlotView slot)
        {
            if (slot == null) return;
            _currentSlot = slot;
            _currentChunk = null;
            Transform anchor = slot != null ? slot.StackAnchor : null;
            transform.SetParent(anchor != null ? anchor : slot?.transform, false);
            transform.localPosition = Vector3.zero;
        }

        public bool TryPlaceOnChunk(HexFieldChunk chunk)
        {
            if (chunk == null) return false;
            if (!chunk.TryAcceptStack(this))
            {
                ReturnToSlot();
                return false;
            }

            _currentChunk = chunk;
            if (_currentSlot != null)
            {
                _currentSlot.ClearSlot();
                _currentSlot = null;
            }

            Transform anchor = chunk.StackAnchor != null ? chunk.StackAnchor : chunk.transform;
            transform.SetParent(anchor, false);
            transform.localPosition = Vector3.zero;
            return true;
        }

        public void ReturnToSlot()
        {
            if (_currentSlot == null) return;
            AttachToSlot(_currentSlot);
        }

        public Vector3 GetElementLocalPosition(int orderIndex)
        {
            return new Vector3(0, orderIndex * _elementHeightStep, 0);
        }
        

        public bool TryTransferTopRunTo(HexStack targetStack, EHexType type, int count)
        {
            if (targetStack == null || _hexStackItemsInfo.Count == 0 || count <= 0)
            {
                return false;
            }

            HexStackItemInfo topRun = _hexStackItemsInfo[_hexStackItemsInfo.Count - 1];
            if (topRun.Type != type || topRun.Length < count)
            {
                return false;
            }

            int startIndex = topRun.StartIndex + topRun.Length - count;
            List<HexElement> movingElements = new List<HexElement>(count);
            for (int i = startIndex; i < startIndex + count; i++)
            {
                movingElements.Add(_elements[i]);
            }

            _elements.RemoveRange(startIndex, count);
            targetStack.AcceptElementsFromNeighbor(movingElements);

            RebuildRuns();
            targetStack.RebuildRuns();
            UpdateElementLayout();
            targetStack.UpdateElementLayout();
            return true;
        }

        private void AcceptElementsFromNeighbor(List<HexElement> newElements)
        {
            if (newElements == null || newElements.Count == 0) return;
            foreach (var element in newElements)
            {
                _elements.Add(element);
                element.AttachToStack(this, _elements.Count - 1);
            }
        }

        private void ApplyRuns(IReadOnlyList<HexStackItem> items)
        {
            if (items == null || items.Count == 0)
            {
                RebuildRuns();
                return;
            }

            _hexStackItemsInfo.Clear();
            int index = 0;
            foreach (var item in items)
            {
                if (!item.IsValid)
                {
                    continue;
                }
                
                int length = Mathf.Min(item.Count, _elements.Count - index);
                
                if (length <= 0)
                {
                    break;
                }

                _hexStackItemsInfo.Add(new HexStackItemInfo(item.HexType, index, length));
                index += length;
                
                if (index >= _elements.Count)
                {
                    break;
                }
            }

            if (_hexStackItemsInfo.Count == 0)
            {
                RebuildRuns();
            }
        }

        private void RebuildRuns()
        {
            _hexStackItemsInfo.Clear();
            if (_elements.Count == 0) return;

            EHexType currentColor = _elements[0].Type;
            int runStart = 0;
            int runLength = 1;
            for (int i = 1; i < _elements.Count; i++)
            {
                if (_elements[i].Type == currentColor)
                {
                    runLength++;
                }
                else
                {
                    _hexStackItemsInfo.Add(new HexStackItemInfo(currentColor, runStart, runLength));
                    currentColor = _elements[i].Type;
                    runStart = i;
                    runLength = 1;
                }
            }
            _hexStackItemsInfo.Add(new HexStackItemInfo(currentColor, runStart, runLength));
        }

        private void UpdateElementLayout()
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                HexElement element = _elements[i];
                if (element == null) continue;
                element.AttachToStack(this, i);
            }
        }

        private void ReleaseElementsToPool()
        {
            if (_elements.Count == 0) return;
            if (_poolingSystem != null)
            {
                foreach (var element in _elements)
                {
                    if (element != null)
                    {
                        _poolingSystem.DestroyAPS(element.gameObject);
                    }
                }
            }

            _elements.Clear();
            _hexStackItemsInfo.Clear();
        }

        public void Initilize()
        {
            // No-op. Configuration happens in SetStack.
        }

        public void Dispose()
        {
            ReleaseElementsToPool();
            _currentSlot = null;
            _currentChunk = null;
        }

        public readonly struct HexStackItemInfo
        {
            public readonly EHexType Type;
            public readonly int StartIndex;
            public readonly int Length;

            public HexStackItemInfo(EHexType type, int startIndex, int length)
            {
                Type = type;
                StartIndex = startIndex;
                Length = length;
            }
        }
    }
}