using UnityEngine;

namespace Code.Core.Views
{
    public class CoreView : MonoBehaviour
    {
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private Transform _slotsRoot;
        
        public Transform LevelRoot => _levelRoot;
        public Transform SlotsRoot => _slotsRoot;
    }
}