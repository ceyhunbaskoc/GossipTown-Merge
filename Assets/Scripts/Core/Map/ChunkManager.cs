using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.CameraSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core.Map
{
    public class ChunkManager : MonoBehaviour, IDisposable
    {
        [Header("Grid Settings")]
        [Tooltip("5 x TilePixel (16.38)")]
        [SerializeField] private float _chunkWorldSize = 20.48f;
        [SerializeField] private Vector2Int _maxGridSize = new Vector2Int(5, 5);

        [Header("Chunk Tile Settings")]
        [SerializeField] private SpriteRenderer _chunkContainerPrefab;
        [Space]
        [SerializeField] private int _chunkBuffer = 1;
        
        [SerializeField] private Transform _chunkContainer;
        
        [Header("Pool")]
        [SerializeField] private int _initialPoolSize = 15;

        private bool _isActive = true;
        private readonly Queue<SpriteRenderer> _containerPool = new Queue<SpriteRenderer>();
        private readonly Dictionary<Vector2Int, SpriteRenderer> _activeContainers = new Dictionary<Vector2Int, SpriteRenderer>();
        private readonly Dictionary<Vector2Int, AsyncOperationHandle<Sprite>> _activeHandles = new Dictionary<Vector2Int, AsyncOperationHandle<Sprite>>();

        private MapCameraController _cameraController;
        private Vector2 _gridWorldOffset;
        
        private TaskCompletionSource<bool> _initialLoadTcs;
        private int _pendingChunkLoads = 0;
        private bool _isInitialLoadDone = false;
        

        public void Initialize(MapCameraController cameraController)
        {
            _cameraController = cameraController;
            
            CalculateGridOffset();

            _cameraController.OnCameraMoved += HandleCameraMoved;
            
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewContainerForPool();
            }
        }
        
        public Task WaitForInitialLoadAsync()
        {
            if (_isInitialLoadDone) return Task.CompletedTask;
    
            _initialLoadTcs = new TaskCompletionSource<bool>();
            return _initialLoadTcs.Task;
        }
        
        private void CalculateGridOffset()
        {
            float totalWorldWidth = _maxGridSize.x * _chunkWorldSize;
            float totalWorldHeight = _maxGridSize.y * _chunkWorldSize;

            _gridWorldOffset = new Vector2(totalWorldWidth / 2f, totalWorldHeight / 2f);
        }

        private void HandleCameraMoved(Vector3 cameraPosition, float orthoSize, float aspect)
        {
            if (!_isActive) return;
            float cameraHalfHeight = orthoSize;
            float cameraHalfWidth = cameraHalfHeight * aspect;
            float minWorldX = cameraPosition.x - cameraHalfWidth;
            float maxWorldX = cameraPosition.x + cameraHalfWidth;
            float minWorldY = cameraPosition.y - cameraHalfHeight;
            float maxWorldY = cameraPosition.y + cameraHalfHeight;
            Vector2Int minGrid = WorldPositionToGridCoord(new Vector3(minWorldX, minWorldY, 0));
            Vector2Int maxGrid = WorldPositionToGridCoord(new Vector3(maxWorldX, maxWorldY, 0));
            int startX = Mathf.Clamp(minGrid.x - _chunkBuffer, 0, _maxGridSize.x - 1);
            int endX = Mathf.Clamp(maxGrid.x + _chunkBuffer, 0, _maxGridSize.x - 1);
            int startY = Mathf.Clamp(minGrid.y - _chunkBuffer, 0, _maxGridSize.y - 1);
            int endY = Mathf.Clamp(maxGrid.y + _chunkBuffer, 0, _maxGridSize.y - 1);

            List<Vector2Int> requiredCoords = new List<Vector2Int>();

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    requiredCoords.Add(new Vector2Int(x, y));
                }
            }
            UpdateVisibleChunks(requiredCoords);
        }
        
        public void SetMapActive(bool isActive)
        {
            if (_isActive == isActive) return;
            _isActive = isActive;
            if (_chunkContainer != null)
            {
                _chunkContainer.gameObject.SetActive(isActive);
            }
        }
        private void UpdateVisibleChunks(List<Vector2Int> requiredCoords)
        {
            List<Vector2Int> coordsToUnload = new List<Vector2Int>();
            foreach (var activeCoord in _activeContainers.Keys)
            {
                if (!requiredCoords.Contains(activeCoord))
                {
                    coordsToUnload.Add(activeCoord);
                }
            }

            foreach (var coord in coordsToUnload)
            {
                UnloadChunk(coord);
            }
            foreach (var coord in requiredCoords)
            {
                if (!_activeContainers.ContainsKey(coord) && !_activeHandles.ContainsKey(coord))
                {
                    LoadChunkAsync(coord);
                }
            }
        }

        private void LoadChunkAsync(Vector2Int coord)
        {
            string address = $"MapTile_{coord.x}_{coord.y}";

            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(address);
            _activeHandles.Add(coord, handle);
    
            _pendingChunkLoads++;

            handle.Completed += (opHandle) =>
            {
                _pendingChunkLoads--;

                if (!_activeHandles.ContainsKey(coord))
                {
                    Addressables.Release(opHandle);
                }
                else if (opHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    SpriteRenderer container = GetContainerFromPool();
                    container.sprite = opHandle.Result;
                    container.transform.position = GridCoordToWorldPosition(coord);
                    container.gameObject.name = address;
            
                    _activeContainers.Add(coord, container);
                }
                else
                {
                    _activeHandles.Remove(coord);
                }

                if (!_isInitialLoadDone && _pendingChunkLoads == 0)
                {
                    _isInitialLoadDone = true;
                    _initialLoadTcs?.TrySetResult(true);
                }
            };
        }

        private void UnloadChunk(Vector2Int coord)
        {
            if (_activeContainers.TryGetValue(coord, out SpriteRenderer container))
            {
                container.sprite = null;
                container.gameObject.SetActive(false);
                _containerPool.Enqueue(container);
                _activeContainers.Remove(coord);
            }

            if (_activeHandles.TryGetValue(coord, out AsyncOperationHandle<Sprite> handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                _activeHandles.Remove(coord);
            }
        }

        private SpriteRenderer GetContainerFromPool()
        {
            if (_containerPool.Count == 0)
            {
                CreateNewContainerForPool();
            }

            SpriteRenderer container = _containerPool.Dequeue();
            container.gameObject.SetActive(true);
            return container;
        }

        private void CreateNewContainerForPool()
        {
            SpriteRenderer newContainer = Instantiate(_chunkContainerPrefab, _chunkContainer);
            newContainer.gameObject.SetActive(false);
            _containerPool.Enqueue(newContainer);
        }

        private Vector2Int WorldPositionToGridCoord(Vector3 worldPos)
        {
            float adjustedX = worldPos.x + _gridWorldOffset.x;
            float adjustedY = worldPos.y + _gridWorldOffset.y;

            return new Vector2Int(
                Mathf.FloorToInt(adjustedX / _chunkWorldSize),
                Mathf.FloorToInt(adjustedY / _chunkWorldSize)
            );
        }

        private Vector3 GridCoordToWorldPosition(Vector2Int coord)
        {
            float halfSize = _chunkWorldSize / 2f;
            
            float xPos = (coord.x * _chunkWorldSize) + halfSize - _gridWorldOffset.x;
            float yPos = (coord.y * _chunkWorldSize) + halfSize - _gridWorldOffset.y;
            
            return new Vector3(xPos, yPos, 0f);
        }

        public void Dispose()
        {
            if (_cameraController != null)
            {
                _cameraController.OnCameraMoved -= HandleCameraMoved;
            }

            foreach (var handle in _activeHandles.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            
            _activeHandles.Clear();
            _activeContainers.Clear();
            _containerPool.Clear();
        }
    }
}