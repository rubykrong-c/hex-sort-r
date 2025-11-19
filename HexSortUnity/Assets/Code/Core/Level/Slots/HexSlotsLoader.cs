using System.Collections.Generic;
using UnityEngine;

namespace Code.Core.Slots
{
    public class HexSlotsLoader
    {
        private readonly PoolingSystem _poolingSystem;
        private readonly HexSlotsHandler _slotsHandler;
        private readonly Transform _containerTransform;


        private const string SLOT_POOL_KEY = "HexSlot";
        private const int SLOTS_COUNT = 3;
        private const float SLOTS_SNAP = 1.7f;

        private readonly List<HexSlotView> _currentSlots = new();

        public HexSlotsLoader(
            PoolingSystem poolingSystem, 
            HexSlotsHandler slotsHandler,
            Transform containerTransform)
        {
            _poolingSystem = poolingSystem;
            _slotsHandler = slotsHandler;
            _containerTransform = containerTransform;
        }

        public void LoadSlots()
        {
            foreach (var slot in _currentSlots)
            {
                if (slot != null)
                {
                    _poolingSystem.DestroyAPS(slot.gameObject);
                }
            }
            _currentSlots.Clear();

            for (int i = 0; i < SLOTS_COUNT; i++)
            {
                HexSlotView slotView = _poolingSystem.InstantiateAPS(SLOT_POOL_KEY).GetComponent<HexSlotView>();
                slotView.transform.SetParent(_containerTransform, false);
                float slotSizeWithSnap = slotView.Size + SLOTS_SNAP;
                slotView.transform.localPosition = new Vector3(
                    i * slotSizeWithSnap - SLOTS_COUNT * slotSizeWithSnap / 2f + slotSizeWithSnap / 2f,
                    slotView.Size / 4f,
                    0f);
                _currentSlots.Add(slotView);
            }

            _slotsHandler.SetSlots(_currentSlots);
        }
    }
}