using System.Collections.Generic;
using Code.Core.Level.Slots.Stack;
using Code.Core.Slots.Deck;

namespace Code.Core.Slots
{
    public class HexSlotsHandler
    {
        private readonly HexDeckHandler _deckHandler;
        private readonly List<HexSlotView> _currentSlots = new();
        private int _freeSlots;

        public HexSlotsHandler(HexDeckHandler deckHandler)
        {
            _deckHandler = deckHandler;
        }

        public void SetSlots(List<HexSlotView> slots)
        {
            foreach (var slot in _currentSlots)
            {
                if (slot != null)
                {
                    slot.SlotCleared -= OnSlotCleared;
                }
            }

            _currentSlots.Clear();
            _currentSlots.AddRange(slots);
            _freeSlots = _currentSlots.Count;

            foreach (var slot in _currentSlots)
            {
                slot.ClearSlot(false);
                slot.SlotCleared += OnSlotCleared;
            }
        }

        public void SpawnStacks()
        {
            foreach (var slot in _currentSlots)
            {
                if (slot == null || !slot.IsFree)
                {
                    continue;
                }
                
                HexStack stack = _deckHandler.TryGetNextStack(true);
                if (stack == null)
                {
                    continue;
                }
                slot.AcceptStack(stack);
                _freeSlots--;
            }
        }

        private void OnSlotCleared(HexSlotView slotView)
        {
            _freeSlots++;
            if (_freeSlots == _currentSlots.Count)
            {
                SpawnStacks();
            }
        }
    }
}