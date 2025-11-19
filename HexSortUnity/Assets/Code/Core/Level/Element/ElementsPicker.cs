using Code.Core.Input;
using Code.Utils;
using UnityEngine;

namespace Code.Core.Level.Element
{
 public class ElementsPicker
    {
        private const int ELEMENTS_STACK_LAYER_MASK = 1 << 6;
        private const int PLANE_LAYER_MASK = 1 << 8;
        private const float MAX_RAYCAST_DISTANCE = 100f;

        private readonly IInputHandler _inputHandler;
        private readonly CameraContainer _cameraContainer;
        
        private Draggable _tempDraggable;
        private bool _isPickingEnable = true;

        public ElementsPicker(IInputHandler inputHandler,
                              CameraContainer cameraContainer)
        {
            _inputHandler = inputHandler;
            _cameraContainer = cameraContainer;
            
            _inputHandler.SetPointerDownCallback(TryToSelectElementWithPointerPosition);
            _inputHandler.SetPointerUpCallback(DeselectCurrentElement);
            _inputHandler.SetDragCallback(DragCurrentElement);
            _inputHandler.SetDragEndCallback(StopDraggingCurrentElement);
        }

        private void TryToSelectElementWithPointerPosition(Vector3 tapPosition)
        {
            if (!_isPickingEnable)
            {
                return;
            }

            if (_tempDraggable != null)
            {
                return;
            }

            Ray inputRay = _cameraContainer.Camera.ScreenPointToRay(tapPosition);

#if UNITY_EDITOR
            Debug.DrawRay(inputRay.origin, inputRay.direction * MAX_RAYCAST_DISTANCE, Color.red, 50);
#endif

            if (!RaycastHelper.RaycastThroughObstacles(inputRay, MAX_RAYCAST_DISTANCE, ELEMENTS_STACK_LAYER_MASK, out var hit))
            {
                return;
            }

            Draggable draggable = hit.collider.gameObject.GetComponent<Draggable>();
            bool isAvailableToDrag = draggable.TryToSelect();
            if (isAvailableToDrag)
            {
                _tempDraggable = draggable;
                Vector3 worldPos = ScreenToWorldPoint(tapPosition);
                _tempDraggable.SetPickedPosition(worldPos);
            }
        }

        private void DeselectCurrentElement()
        {
            if (_tempDraggable == null)
            {
                return;
            }

            if (_tempDraggable.TryToDeselect())
            {
                _tempDraggable = null;
            }
        }

        private void DragCurrentElement(Vector3 tapPosition)
        {
            if (_tempDraggable == null)
            {
                return;
            }

            Vector3 worldPoint = ScreenToWorldPoint(tapPosition);
            _tempDraggable.OnDrag(worldPoint);
        }

        private void StopDraggingCurrentElement(Vector3 tapPosition)
        {
            if (_tempDraggable == null)
            {
                return;
            }

            _tempDraggable.OnDragEnd();
            _tempDraggable = null;
        }

        private Vector3 ScreenToWorldPoint(Vector3 tapPosition)
        {
            Vector3 screenPoint = tapPosition;
            Ray inputRay = _cameraContainer.Camera.ScreenPointToRay(screenPoint);

            screenPoint.z = Physics.Raycast(inputRay, out var hit, MAX_RAYCAST_DISTANCE, PLANE_LAYER_MASK)
                ? hit.distance
                : MAX_RAYCAST_DISTANCE;

            return _cameraContainer.Camera.ScreenToWorldPoint(screenPoint);
        }
    }
}