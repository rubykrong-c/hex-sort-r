using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Core.Input
{
    public interface IInputHandler
    {
        void SetActiveInputHandler(bool active);
        void SetPointerDownCallback(Action<Vector3> callback);
        void SetPointerUpCallback(Action callback);
        void SetDragCallback(Action<Vector3> callback);
        void SetDragEndCallback(Action<Vector3> callback);
    }

    public class InputHandler : MonoBehaviour, IInputHandler,
        IPointerDownHandler, IPointerUpHandler,
        IDragHandler, IEndDragHandler
    {
#pragma warning disable 0649

        [SerializeField] private Image _image;


#pragma warning restore 0649

        private int _firstTouchId;
        
        public void SetActiveInputHandler(bool active)
        {
            _image.raycastTarget = active;
        }

        private Action<Vector3> _onPointerDownCallback;
        private Action _onPointerUpCallback;
        private Action<Vector3> _onDragCallback;
        private Action<Vector3> _onDragEndCallback;

        public void SetPointerDownCallback(Action<Vector3> callback)
        {
            _onPointerDownCallback = callback;
        }

        public void SetPointerUpCallback(Action callback)
        {
            _onPointerUpCallback = callback;
        }

        public void SetDragCallback(Action<Vector3> callback)
        {
            _onDragCallback = callback;
        }

        public void SetDragEndCallback(Action<Vector3> callback)
        {
            _onDragEndCallback = callback;
        }

        public async void OnPointerDown(PointerEventData eventData)
        {

            if (!IsTheFirstFingerInput(eventData))
                return;

            _onPointerDownCallback(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsTheFirstFingerInput(eventData))
                return;

            _onPointerUpCallback?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsTheFirstFingerInput(eventData))
                return;

            _onDragCallback?.Invoke(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsTheFirstFingerInput(eventData))
                return;

            _onDragEndCallback?.Invoke(eventData.position);
        }

        private bool IsTheFirstFingerInput(PointerEventData eventData)
        {
#if UNITY_EDITOR
            return true;
#endif
            if (UnityEngine.Input.touchCount == 1)
                return true;

            if (UnityEngine.Input.touches[0].fingerId == eventData.pointerId)
                return true;

            return false;
        }
    }
}