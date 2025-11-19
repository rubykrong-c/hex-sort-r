using System;
using DG.Tweening;
using UnityEngine;

namespace Code.Core
{
    public class TileView: MonoBehaviour
    {
        
        [Header("Selection")]
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _baseMaterial;
        [SerializeField] private Material _highlightMaterial;
        
        public Transform ItemSpawningTransform => transform;
        public bool IsFree { get; private set; } = true;
        public bool IsAccessible { get; private set; } = true;
        public Action OnOccupied;
        
        public void SetElement()
        {
            if (!IsFree) return;
            IsFree = false;
            OnOccupied?.Invoke();
        }
        public void HighlightTile(bool highlight)
        {
            
            if (IsAccessible)
            {
                DOTween.KillAll();
                 _meshRenderer.material = highlight ? _highlightMaterial : _baseMaterial;
                
            }
        }
    }
}