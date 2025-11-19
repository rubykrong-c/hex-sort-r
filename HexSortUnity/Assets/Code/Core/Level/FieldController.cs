using System.Collections.Generic;
using System.Linq;
using Code.Core.Level.Slots.Stack;
using Code.Core.Slots.Stack;
using UnityEngine;

namespace Code.Core.Level
{

    public class FieldController
    {
        private readonly FieldModel _fieldModel;
        private readonly List<FieldMatch> _matches = new();
        private readonly HashSet<FieldModel.FieldHex> _occupiedMatches = new();
        private readonly HashSet<(FieldModel.FieldHex, FieldModel.FieldHex)> _checkedPairs = new();

        public IReadOnlyList<FieldMatch> Matches => _matches;

        public FieldController(FieldModel fieldModel)
        {
            _fieldModel = fieldModel;
        }

        public void RegisterStack(HexStack stack)
        {
            if (stack == null)
            {
                return;
            }

            stack.OnChunkSet -= HandleStackPlaced;
            stack.OnChunkSet += HandleStackPlaced;
            stack.OnSort -= SortStacks;
            stack.OnSort += SortStacks;
        }

        private void SortStacks(HexStack stack, HexStack stack2)
        {
            UpdateHex(stack);
            UpdateHex(stack2);
            EvaluateMatches();
        }

        private void UpdateHex(HexStack stack)
        {
            if (stack.TryGetTopRun(out var topColor, out int count))
            {
                _fieldModel.UpdateHex(stack.CurrentTile, stack, topColor, count);
            }
        }
        
        private void HandleStackPlaced(HexStack stack)
        {
            if (stack == null)
            {
                return;
            }

            var tile = stack.CurrentTile;
            if (tile == null)
            {
                return;
            }

            if (stack.TryGetTopRun(out var topColor, out int count))
            {
                _fieldModel.UpdateHex(tile, stack, topColor, count);
            }
            else
            {
                _fieldModel.UpdateHex(tile, stack, default, 0);
            }

            EvaluateMatches();
         
        }

        private void EvaluateMatches()
        {
            _matches.Clear();
            _occupiedMatches.Clear();
            _checkedPairs.Clear();

            foreach (var hex in _fieldModel.Hexes)
            {
                if (!hex.HasStack || hex.TopColorCount <= 0)
                {
                    continue;
                }

                foreach (var neighbor in hex.Neighbors)
                {
                    if (!neighbor.HasStack || neighbor.TopColorCount <= 0)
                    {
                        continue;
                    }

                    if (neighbor.TopColor != hex.TopColor)
                    {
                        continue;
                    }
                    
                    var pair = hex.GetHashCode() < neighbor.GetHashCode()
                        ? (hex, neighbor)
                        : (neighbor, hex);

                    if (_checkedPairs.Contains(pair))
                        continue;

                    _checkedPairs.Add(pair);

                    var preferred = hex;
                    var secondary = neighbor;
                    if (neighbor.TopColorCount > hex.TopColorCount)
                    {
                        preferred = neighbor;
                        secondary = hex;
                    }

                    if (_occupiedMatches.Contains(preferred))
                    {
                        continue;
                    }

                    _occupiedMatches.Add(preferred);
                    _matches.Add(new FieldMatch(secondary, preferred));
                }
            }
            
            
            if(_matches.Count > 0)
            {
                var v = _matches.First();
                v.FromHex.Stack.Jump(v.ToHex.Stack, v.FromHex.TopColor, v.FromHex.TopColorCount); 
            }
        }

        public readonly struct FieldMatch
        {
            public readonly FieldModel.FieldHex FromHex;
            public readonly FieldModel.FieldHex ToHex;

            public FieldMatch(FieldModel.FieldHex fromHex, FieldModel.FieldHex toHex)
            {
                FromHex = fromHex;
                ToHex = toHex;
            }
        }
    }
}
