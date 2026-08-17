using Core.GridSystem;
using UnityEngine;

namespace Core.Services
{
    public class BoardTransferService
    {
        private readonly GridDataModel _mainGrid;
        private readonly GridDataModel _backpackGrid;

        public BoardTransferService(GridDataModel mainGrid, GridDataModel backpackGrid)
        {
            _mainGrid = mainGrid;
            _backpackGrid = backpackGrid;
        }

        public bool TrySendToBackpack(Vector2Int mainGridSourcePos)
        {
            IGridItem item = _mainGrid.GetItemAt(mainGridSourcePos);
            if (item == null) return false;

            if (TryGetFirstEmptySlot(_backpackGrid, out Vector2Int emptyBackpackPos))
            {
                if (_backpackGrid.TryPlaceObject(emptyBackpackPos, item))
                {
                    _mainGrid.TryClearCell(mainGridSourcePos);
                    return true;
                }
            }
            
            Debug.LogWarning("Çanta dolu!");
            return false;
        }

        public bool CanSendToBackpack(Vector2Int mainGridSourcePos)
        {
            IGridItem item = _mainGrid.GetItemAt(mainGridSourcePos);
            if (item == null) return false;
            if (TryGetFirstEmptySlot(_backpackGrid, out Vector2Int emptyBackpackPos))
            {
                return true;
            }

            return false;
        }

        public bool TryRetrieveFromBackpack(Vector2Int backpackSourcePos)
        {
            IGridItem item = _backpackGrid.GetItemAt(backpackSourcePos);
            if (item == null) return false;

            if (TryGetFirstEmptySlot(_mainGrid, out Vector2Int emptyMainGridPos))
            {
                if (_mainGrid.TryPlaceObject(emptyMainGridPos, item))
                {
                    _backpackGrid.TryClearCell(backpackSourcePos);
                    return true;
                }
            }

            return false;
        }

        private bool TryGetFirstEmptySlot(GridDataModel grid, out Vector2Int emptyPos)
        {
            emptyPos = default;
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!grid.IsCellOccupied(pos) && !grid.IsCellLocked(pos))
                    {
                        emptyPos = pos;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}