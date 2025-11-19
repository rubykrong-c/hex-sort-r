using UnityEngine;

namespace Code.Core
{
    public class CameraContainer: MonoBehaviour
    {
        [SerializeField] Camera _targetCamera;

        public Camera Camera => _targetCamera != null ? _targetCamera : Camera.main;
    }
}