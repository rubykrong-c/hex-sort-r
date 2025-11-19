using Code.Core.Level.Slots.Stack;
using UnityEngine;

namespace Code
{
    public class HexFieldChunk : MonoBehaviour
    {
        [SerializeField]
        private Transform _stackAnchor;

        private HexStack _currentStack;

        public Transform StackAnchor => _stackAnchor != null ? _stackAnchor : transform;
        public bool IsBusy => _currentStack != null;

        public bool TryAcceptStack(HexStack stack)
        {
            if (_currentStack != null) return false;
            _currentStack = stack;
            return true;
        }

        public void Clear(HexStack stack)
        {
            if (_currentStack == stack)
            {
                _currentStack = null;
            }
        }
    }
}