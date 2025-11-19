using System;
using System.Collections.Generic;
using Code.Core.Level.Element;
using Code.Core.Slots;
using Code.Core.Slots.Stack;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Code.Core.Level.Slots.Stack
{
    public class HexStack : MonoBehaviour, IPoolable
    {
        private const float OFFSET_Y_ANIMATION = 2f;
        private const float SPEED_ANIMATION = 14f;
        private const float THRESHOLD_DESTINATION_ANIM = 4.5f;
        private const float MOVE_TO_CHUNCK_ANIMATION_TIME = 0.2f;
        
        [SerializeField] private Transform _elementsRoot;
        [SerializeField] private float _elementHeightStep = 0.05f;
        [SerializeField] private bool _enableElementColliders = true;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Draggable _draggable;
        
        public bool IsInSlot { get; private set; }
        
        private readonly List<HexElement> _elements = new();
        private readonly List<HexStackItemInfo> _hexStackItemsInfo = new();
        public Action<HexStack> OnChunkSet;

        private PoolingSystem _poolingSystem;
        private HexSlotView _currentSlot;
        private TileView _currentTile;
        private bool _isMoveAnimationRunning;

        public Transform ElementsRoot => _elementsRoot;

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
                }
            }

            ApplyRuns(orderedHexStackItems);
            UpdateElementLayout();
            UpdateStackCollider();
        }

        public async UniTaskVoid AttachToSlot(HexSlotView slot)
        {
            if (slot == null)
            {
                return;
            }
            _currentSlot = slot;
            _currentTile = null;
            Transform anchor = slot != null ? slot.StackAnchor : null;
            transform.SetParent(anchor != null ? anchor : slot?.transform, false);
            transform.localPosition = Vector3.zero;
            
            _draggable.MoveBackInSlot = () => MoveToSlot(slot);
            _draggable.MoveInChunk = x => SetChunk(x, false);
           // MoveToSlotInitial(slotItem, slotIndex);
            await UniTask.WaitWhile(() => _isMoveAnimationRunning == true);
            _draggable.EnableDrag(true);
        }

        private void MoveToSlot(HexSlotView slot)
        {
            _draggable.EnableCollider(false);
            transform.DOKill();

            var dest = Vector3.Distance(slot.transform.position, transform.position);

            var time = dest / SPEED_ANIMATION;
            if (dest > THRESHOLD_DESTINATION_ANIM)
            {
                time = THRESHOLD_DESTINATION_ANIM / SPEED_ANIMATION;
            }

            var worldPos = slot.transform.position;
            MoveToPos(worldPos, time);
        }

        private async UniTaskVoid SetChunk(TileView tileView, bool isInitialSet = true)
        {
            IsInSlot = false;
            _draggable.EnableDrag(false);
            transform.SetParent(tileView.ItemSpawningTransform, true);
            await UniTask.DelayFrame(1);
            await transform.DOLocalMove(Vector3.zero, MOVE_TO_CHUNCK_ANIMATION_TIME).SetEase(Ease.InBack).ToUniTask();
            
            _currentTile = tileView;
            _currentTile.SetElement();

            if (_currentSlot != null)
            {
                _draggable.MoveBackInSlot = null;
                _currentSlot.ClearSlot();
                _currentSlot = null;
            }

            await UniTask.DelayFrame(1);
           
            OnChunkSet?.Invoke(this);
        }
        
        private void MoveToPos(Vector3 pos, float time)
        {
            _isMoveAnimationRunning = true;
            transform.DOMoveY(pos.y + OFFSET_Y_ANIMATION, time * 0.3f).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                transform.DOMoveY(pos.y, time * 0.7f).SetEase(Ease.Linear);
            });

            transform.DOMoveX(pos.x, time).SetEase(Ease.InQuad);
            transform.DOMoveZ(pos.z, time).SetEase(Ease.InBack).OnComplete(() =>
            {
                _isMoveAnimationRunning = false;
                _draggable.EnableDrag(true);
            });
        }

        public Vector3 GetElementLocalPosition(int orderIndex)
        {
            return new Vector3(0, orderIndex * _elementHeightStep, 0);
        }
        
        private void UpdateStackCollider()
        {
            if (_collider == null || _elements.Count == 0)
            {
                return;
            }

            Bounds combined = new Bounds();

            bool initialized = false;
            foreach (var element in _elements)
            {
                if (element == null)
                {
                    continue;
                }

                var elementMesh = element.Mesh;

                if (!initialized)
                {
                    combined = elementMesh.bounds;
                    initialized = true;
                }
                else
                {
                    combined.Encapsulate(elementMesh.bounds);
                }
            }

            if (!initialized)
            {
                return;
            }

            // Переводим мировые координаты в локальные стека
            Vector3 localCenter = transform.InverseTransformPoint(combined.center);
            Vector3 localSize = transform.InverseTransformVector(combined.size);

            _collider.center = localCenter;
            _collider.size = localSize;
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
                if (element == null)
                {
                    continue;
                }
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
        }

        public void Dispose()
        {
            ReleaseElementsToPool();
            _currentSlot = null;
            _currentTile = null;
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