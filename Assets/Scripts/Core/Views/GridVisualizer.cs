using System.Collections.Generic;
using Core.GridSystem;
using Core.PoolSystem;
using UnityEngine;

namespace Core.Views
{
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private Transform _boardContainer;
        [SerializeField] private Color _firstColor;
        [SerializeField] private Color _secondColor;
        
        private IObjectPool _objectPool;

        private Color currentColor;
        
        public Vector3 GridOrigin { get; private set; }
        
        public void Initialize(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            currentColor = _firstColor;
        }

        public Dictionary<Vector2Int, IViewCell> DrawBoard(int width, int height, float cellSize)
        {
            var cellDictionary = new Dictionary<Vector2Int, IViewCell>();
            
            float gridWidth = width * cellSize;
            float gridHeight = height * cellSize;
            float halfCellOffset = cellSize / 2f;

            GridOrigin = _boardContainer.position - new Vector3(gridWidth / 2f, gridHeight / 2f, 0f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    Vector3 worldPos = GridOrigin + new Vector3(
                        (x * cellSize) + halfCellOffset, 
                        (y * cellSize) + halfCellOffset, 
                        0f
                    );
                    GameObject cellVisual = _objectPool.Spawn(PoolObjectType.CellVisual, worldPos, Quaternion.identity, _boardContainer);
                    if(cellVisual.TryGetComponent<CellVisual>(out CellVisual cellComponent))
                    {
                        cellDictionary.Add(pos, cellComponent);
                        cellComponent.SetBackgroundColor(currentColor); 
                        currentColor = currentColor == _firstColor ? _secondColor : _firstColor;
                    }
                    else
                    {
                        Debug.LogError("CellVisual component not found on the spawned object.");
                    }
                }
            }
            return cellDictionary;
        }
    
    }
}