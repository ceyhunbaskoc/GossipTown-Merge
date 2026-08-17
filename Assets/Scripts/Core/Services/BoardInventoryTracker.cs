using System;
using System.Collections.Generic;
using Core.GridSystem;
using Order;
using UnityEngine;

namespace Core.Services
{
    public class BoardInventoryTracker : IDisposable, IBoardInventoryProvider
    {
        private readonly GridDataModel _gridModel;
        private readonly OrderDataModel _orderDataModel;
        
        private readonly Dictionary<ItemIdentifier, int> _cachedInventory = new Dictionary<ItemIdentifier, int>();

        public BoardInventoryTracker(GridDataModel gridModel, OrderDataModel orderDataModel)
        {
            _gridModel = gridModel;
            _orderDataModel = orderDataModel;

            _gridModel.OnItemPlaced += HandleBoardChanged;
            _gridModel.OnItemSpawned += HandleBoardChangedSpawn;
            _gridModel.OnCellCleared += HandleBoardCleared;
            
            _orderDataModel.OnOrderCreated += HandleOrderCreated;
            RecalculateBoard();
        }
        
        private void HandleOrderCreated(OrderModel newOrder) => RecalculateBoard();
        private void HandleBoardChanged(Vector2Int pos, IGridItem item) => RecalculateBoard();
        private void HandleBoardChangedSpawn(Vector2Int from, Vector2Int to, IGridItem item) => RecalculateBoard();
        private void HandleBoardCleared(Vector2Int pos) => RecalculateBoard();

        private void RecalculateBoard()
        {
            _cachedInventory.Clear();

            for (int x = 0; x < _gridModel.Width; x++)
            {
                for (int y = 0; y < _gridModel.Height; y++)
                {
                    Vector2Int currentPos = new Vector2Int(x, y);
                    if (_gridModel.IsCellLocked(currentPos)) continue; 
                    
                    IGridItem item = _gridModel.GetItemAt(currentPos);
                    if (item != null)
                    {
                        ItemIdentifier id = new ItemIdentifier(item.Id, item.Level);
                        if (_cachedInventory.TryGetValue(id, out int currentCount))
                        {
                            _cachedInventory[id] = currentCount + 1;
                        }
                        else
                        {
                            _cachedInventory[id] = 1;
                        }
                    }
                }
            }

            _orderDataModel.SyncAllOrders(_cachedInventory);
        }

        public int GetItemCountOnBoard(ItemIdentifier identifier)
        {
            if (_cachedInventory.TryGetValue(identifier, out int count))
            {
                return count;
            }
            return 0;
        }

        public void Dispose()
        {
            _gridModel.OnItemPlaced -= HandleBoardChanged;
            _gridModel.OnItemSpawned -= HandleBoardChangedSpawn;
            _gridModel.OnCellCleared -= HandleBoardCleared;
            
            _orderDataModel.OnOrderCreated -= HandleOrderCreated;
        }
    }
}