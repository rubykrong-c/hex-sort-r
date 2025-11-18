using UnityEngine;

namespace Code.Core.Views
{
    public class CoreView : MonoBehaviour
    {
        [SerializeField] private Transform _levelRoot;
        
        public Transform LevelRoot => _levelRoot;
    }
}