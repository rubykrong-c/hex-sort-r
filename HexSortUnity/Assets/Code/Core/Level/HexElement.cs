using Code.Core.Level.Slots.Stack;
using Code.Core.Slots.Stack;
using UnityEngine;

namespace Code
{
    public class HexElement : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private MeshRenderer _renderer;
        
        public EHexType Type { get; private set; }
        public HexStack CurrentStack { get; private set; }
        public MeshRenderer Mesh => _renderer;

        public void Configure(EHexType colorId, Material material)
        {
            
            Type =  colorId;
            if (_renderer != null && material != null)
            {
                _renderer.material = material;
            }
        }

        public void AttachToStack(HexStack stack, int orderInStack)
        {
            CurrentStack = stack;
            Transform targetParent = stack.ElementsRoot != null ? stack.ElementsRoot : stack.transform;
            transform.SetParent(targetParent, false);
            transform.localPosition = stack.GetElementLocalPosition(orderInStack);
        }
        
        public void Detach()
        {
            CurrentStack = null;
            transform.SetParent(null, false);
        }

        public void Initilize()
        {
            
        }

        public void Dispose()
        {
            CurrentStack = null;
        }
    }
}