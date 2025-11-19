using System;
using Code.Core.Level.Element;
using Code.Core.Level.Slots.Stack;
using UnityEngine;

namespace Code.Core.Slots
{
    public class HexSlotView : MonoBehaviour
    {
        [SerializeField] private Transform _stackAnchor;
        [SerializeField] private float _slotSize = 1.5f;
        
        public event Action<HexSlotView> SlotCleared;
        public bool IsFree => _currentStack == null;
        public Transform StackAnchor => _stackAnchor != null ? _stackAnchor : transform;
        public float Size => _slotSize;

        private HexStack _currentStack;
        
        public void AcceptStack(HexStack stack)
        {
            _currentStack = stack;
            stack.AttachToSlot(this);
        }

        public void ClearSlot(bool invokeCallback = true)
        {
            if (_currentStack == null)
            {
                return;
            }
            
            _currentStack = null;
            
            if (invokeCallback)
            {
                SlotCleared?.Invoke(this);
            }
        }
    }
}