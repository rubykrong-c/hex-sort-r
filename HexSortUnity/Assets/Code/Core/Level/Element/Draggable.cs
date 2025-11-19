using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Core.Level.Element
{
    public class Draggable : MonoBehaviour
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private SphereCaster _chunksCaster;
        [SerializeField] private Vector3 _offset = new(-0.3f, 0.5f, 0);
        [SerializeField] private bool _isPicked;
        [SerializeField] private bool _isDragging;
        
        public bool IsAvailableToPick { get; private set; }
        public Action MoveBackInSlot;
        public Action<TileView> MoveInChunk;

        private Vector3 _startPosition;
        private TileView _tempCollidingTile;
        private TileView _latestCollidingChunk;

        public void EnableCollider(bool enable)
        {
            _collider.enabled = enable;
        }

        public void EnableDrag(bool enable)
        {
            IsAvailableToPick = enable;
            _collider.enabled = enable;
            _startPosition = transform.position;
        }

        public bool TryToSelect()
        {
            if (!IsAvailableToPick)
            {
                return false;
            }

            if (_isPicked)
            {
                return false;
            }

            _isPicked = true;

            _collider.enabled = false;
            return true;
        }

        public void SetPickedPosition(Vector3 position)
        {
            _chunksCaster.transform.localPosition = _offset;
            FollowInputPosition(position);
        }

        public bool TryToDeselect()
        {
            if (!_isPicked || _isDragging) return false;
            transform.position = _startPosition;
            _isPicked = false;
            _collider.enabled = true;
            return true;
        }

        public void OnDrag(Vector3 position)
        {
            _isDragging = true;
            FollowInputPosition(position);
        }

        public void OnDragEnd()
        {
            if (_tempCollidingTile != null)
            {
                MoveInChunk.Invoke(_tempCollidingTile);
                _tempCollidingTile.HighlightTile(false);
                _tempCollidingTile = null;
                _latestCollidingChunk = null;
            }
            else
            {
                MoveBackInSlot.Invoke();
            }
            
            _isDragging = false;
            _isPicked = false;
        }


        private void FollowInputPosition(Vector3 position)
        {
            Vector3 newPos = position + _offset;

            if (Vector3.Distance(transform.position, newPos) > 0.02f)
            {
                TileView[] collidingChunks = _chunksCaster.CastSphereDown<TileView>(newPos);
                if (collidingChunks.Length != 0)
                {
                    _tempCollidingTile = FindClosestObject(newPos, collidingChunks);
                    if (_tempCollidingTile != _latestCollidingChunk)
                    {
                        if (_tempCollidingTile != null)
                        {
                            _tempCollidingTile.HighlightTile(true);
                        }

                        _latestCollidingChunk?.HighlightTile(false);
                    }

                    _latestCollidingChunk = _tempCollidingTile;
                }
                else
                {
                    // Lost Grid Collision
                    if (_tempCollidingTile != null)
                    {
                        _tempCollidingTile.HighlightTile(false);
                        _tempCollidingTile = null;
                        _latestCollidingChunk = null;
                    }
                }
            }
            float smoothSpeed = 48f;
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * smoothSpeed);
        }

        private TileView FindClosestObject(Vector3 pos1, TileView[] tilesToCheck)
        {
            TileView tempClosestTile = null;
            float tempClosestDistance = float.MaxValue;

            foreach (TileView tile in tilesToCheck)
            {
                if (!tile.IsFree || !tile.IsAccessible) continue;

                float tempDistance = Vector3.Distance(pos1, tile.transform.position);
                if (tempDistance < tempClosestDistance)
                {
                    tempClosestDistance = tempDistance;
                    tempClosestTile = tile;
                }
            }
            return tempClosestTile;
        }
        
    }
}