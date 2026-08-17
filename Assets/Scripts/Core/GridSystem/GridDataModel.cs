using System;
using System.Collections.Generic;
using Core.Rules;
using Core.SaveSystem;
using Order;
using UnityEngine;

namespace Core.GridSystem
{
    public class GridDataModel : IReadOnlyGrid,IGridModifier
    {
        private readonly CellModel[,] _cells;
        public int Width { get;}
        public int Height { get;}
        
        public event Action<Vector2Int, IGridItem> OnItemPlaced;
        public event Action<Vector2Int, Vector2Int, IGridItem> OnItemMoved;
        public event Action<Vector2Int> OnCellCleared;
        public event Action<IReadOnlyList<Vector2Int>> OnCellsUnlocked;
        public event Action<Vector2Int> OnCellUnlocked;
        public event Action<Vector2Int, Vector2Int, IGridItem> OnItemSpawned;

        private Vector2Int[] _directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        public GridDataModel(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new CellModel[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _cells[x,y] = new CellModel(new Vector2Int(x, y));
                }
            }
        }
        
        public void InitializeCellLock(Vector2Int position, bool isLocked)
        {
            if (IsValidBoundary(position))
            {
                if (isLocked)
                {
                    _cells[position.x, position.y] = new CellModel(position, true);
                }
            }
        }

        public bool IsCellOccupied(Vector2Int position)
        {
            if (!IsValidBoundary(position)) return false; 
            return _cells[position.x, position.y].IsOccupied;
        }
        
        public bool IsCellLocked(Vector2Int position) => 
            IsValidBoundary(position) && _cells[position.x, position.y].IsLocked;

        public bool IsCellUnlockable(Vector2Int position)
        {
            foreach (var direction in _directions)
            {
                Vector2Int neighborPos = position + direction;
                if (IsValidBoundary(neighborPos))
                {
                    if (!_cells[neighborPos.x, neighborPos.y].IsLocked)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsValidBoundary(Vector2Int position)
        {
            return position.x>=0 && position.x<Width &&
                   position.y>=0 && position.y<Height;
        }

        public IGridItem GetItemAt(Vector2Int position)
        {
            if (!IsValidBoundary(position)) return null;
            return _cells[position.x, position.y].Item;
        }

        public bool TryPlaceObject(Vector2Int position, IGridItem item)
        {
            if (!IsValidBoundary(position) || IsCellOccupied(position))
                return false;

            _cells[position.x, position.y].SetItem(item);
            OnItemPlaced?.Invoke(position, item);
            return true;
        }

        public bool TryMoveObject(Vector2Int fromPosition, Vector2Int toPosition)
        {
            if (!IsValidBoundary(fromPosition) || !IsValidBoundary(toPosition)) return false;
            if (!IsCellOccupied(fromPosition)) return false;
            
            if (IsCellLocked(fromPosition)) return false;

            if (IsCellOccupied(toPosition)) return false;
            if (IsCellLocked(toPosition)) return false;

            IGridItem item = _cells[fromPosition.x, fromPosition.y].Item;
            _cells[fromPosition.x, fromPosition.y].Clear();
            _cells[toPosition.x, toPosition.y].SetItem(item);
    
            OnItemMoved?.Invoke(fromPosition, toPosition, item);
            return true;
        }
        
        public bool TryClearCell(Vector2Int position)
        {
            if (!IsValidBoundary(position) || !IsCellOccupied(position))
                return false;
            
            _cells[position.x, position.y].Clear();
            OnCellCleared?.Invoke(position);
            return true;
        }
        
        public bool TryUnlockLockedCells(Vector2Int position)
        {
            _cells[position.x, position.y].SetUnlocked();
            List<Vector2Int> successfullyUnlocked = new List<Vector2Int> {position};

            foreach (var dir in _directions)
            {
                Vector2Int neighborPos = position + dir;
        
                if (IsValidBoundary(neighborPos) && IsCellLocked(neighborPos))
                {
                    _cells[neighborPos.x, neighborPos.y].SetUnlocked();
                    successfullyUnlocked.Add(neighborPos);
                }   
            }

            if (successfullyUnlocked.Count > 0)
            {
                OnCellsUnlocked?.Invoke(successfullyUnlocked);
                return true;
            }
            return false;
        }
        
        public bool TryUnlockLockedCell(Vector2Int position)
        {
            if (!IsCellLocked(position)) return false;
            _cells[position.x, position.y].SetUnlocked();
            OnCellUnlocked?.Invoke(position);
            return true;
        }
        
        public bool TrySpawnObject(Vector2Int fromPosition, Vector2Int toPosition, IGridItem item)
        {
            if (!IsValidBoundary(toPosition) || IsCellOccupied(toPosition))
                return false;

            _cells[toPosition.x, toPosition.y].SetItem(item);
    
            OnItemSpawned?.Invoke(fromPosition, toPosition, item);
            return true;
        }

        public Vector2Int? FindNearestEmptyCell(Vector2Int position)
        {
            Vector2Int? nearestEmptyCell = null;
            float minDistanceSqr = float.MaxValue;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector2Int checkPos = new Vector2Int(x, y);
                    if (IsCellOccupied(checkPos) || IsCellLocked(checkPos) || checkPos == position)
                    {
                        continue;
                    }
                    float distanceSqr = (checkPos - position).sqrMagnitude;
                    if (distanceSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distanceSqr;
                        nearestEmptyCell = checkPos;
                    }
                }
            }
            return nearestEmptyCell;
        }
        
        public List<CellSaveData> GetSaveData()
        {
            List<CellSaveData> allCellsData = new List<CellSaveData>();
    
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    allCellsData.Add(_cells[x, y].GetSaveData());
                }
            }
    
            return allCellsData;
        }
        
        public int ToFlatIndex(Vector2Int gridPosition)
        {
            if (gridPosition.x < 0 || gridPosition.x >= Width || gridPosition.y < 0 || gridPosition.y >= Height)
            {
                throw new System.ArgumentOutOfRangeException(nameof(gridPosition), $"Coordinate {gridPosition} is out of bounds!");
            }

            return gridPosition.x + (gridPosition.y * Width);
        }
        
        public Vector2Int ToGridPosition(int flatIndex)
        {
            if (flatIndex < 0 || flatIndex >= Width * Height)
            {
                throw new System.ArgumentOutOfRangeException(nameof(flatIndex), $"Index {flatIndex} is out of bounds!");
            }

            int x = flatIndex % Width;
            int y = flatIndex / Width;
            
            return new Vector2Int(x, y);
        }
    }
}