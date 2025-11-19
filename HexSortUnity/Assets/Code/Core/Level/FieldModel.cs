using System.Collections.Generic;
using Code.Core;
using UnityEngine;

namespace Code.Core.Level
{
    public class FieldModel
    {
        private readonly List<FieldHex> _hexes = new();
        private readonly Dictionary<TileView, FieldHex> _lookup = new();

        public IReadOnlyList<FieldHex> Hexes => _hexes;

        public void Build(IEnumerable<TileView> tiles)
        {
            _hexes.Clear();
            _lookup.Clear();
            if (tiles == null)
            {
                return;
            }

            foreach (var tile in tiles)
            {
                if (tile == null)
                {
                    continue;
                }
                var hex = new FieldHex(tile);
                _hexes.Add(hex);
                _lookup[tile] = hex;
            }

            if (_hexes.Count < 2)
            {
                return;
            }

            float neighborDistance = ResolveNeighborDistance();
            if (neighborDistance <= Mathf.Epsilon)
            {
                return;
            }

            float maxDistance = neighborDistance * 1.2f;
            float maxDistanceSqr = maxDistance * maxDistance;

            for (int i = 0; i < _hexes.Count - 1; i++)
            {
                FieldHex a = _hexes[i];
                Vector2 posA = a.FlatPosition;
                for (int j = i + 1; j < _hexes.Count; j++)
                {
                    FieldHex b = _hexes[j];
                    float sqrDist = (b.FlatPosition - posA).sqrMagnitude;
                    if (sqrDist > maxDistanceSqr)
                    {
                        continue;
                    }

                    a.LinkNeighbor(b);
                    b.LinkNeighbor(a);
                }
            }

        }

        public bool TryGetHex(TileView tile, out FieldHex hex)
        {
            return _lookup.TryGetValue(tile, out hex);
        }

        public IEnumerable<TileView> GetNeighborTiles(TileView tile)
        {
            if (!_lookup.TryGetValue(tile, out var hex))
            {
                yield break;
            }

            foreach (var neighbor in hex.Neighbors)
            {
                yield return neighbor.Tile;
            }
        }

        private float ResolveNeighborDistance()
        {
            float minSqr = float.MaxValue;
            for (int i = 0; i < _hexes.Count - 1; i++)
            {
                Vector2 posA = _hexes[i].FlatPosition;
                for (int j = i + 1; j < _hexes.Count; j++)
                {
                    float sqrDist = (_hexes[j].FlatPosition - posA).sqrMagnitude;
                    if (sqrDist <= Mathf.Epsilon) continue;
                    if (sqrDist < minSqr)
                    {
                        minSqr = sqrDist;
                    }
                }
            }

            return minSqr == float.MaxValue ? 0f : Mathf.Sqrt(minSqr);
        }

        public class FieldHex
        {
            private readonly List<FieldHex> _neighbors = new();

            public TileView Tile { get; }
            public IReadOnlyList<FieldHex> Neighbors => _neighbors;
            internal Vector2 FlatPosition { get; }

            public FieldHex(TileView tile)
            {
                Tile = tile;
                Vector3 worldPos = tile.transform.position;
                FlatPosition = new Vector2(worldPos.x, worldPos.z);
            }

            internal void LinkNeighbor(FieldHex other)
            {
                if (other == null || other == this)
                {
                    return;
                }
                if (_neighbors.Contains(other))
                {
                    return;
                }
                _neighbors.Add(other);
            }
        }
    }
}
