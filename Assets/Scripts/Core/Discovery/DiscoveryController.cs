using System;
using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;

namespace Core.Discovery
{
    public class DiscoveryController : IDisposable
    {
        private readonly GridDataModel _gridDataModel;
        private readonly IItemDiscoveryModifier _discoveryModifier;

        public DiscoveryController(GridDataModel gridDataModel, IItemDiscoveryModifier discoveryModifier)
        {
            _gridDataModel = gridDataModel;
            _discoveryModifier = discoveryModifier;

            _gridDataModel.OnItemPlaced += HandleItemPlaced;
            _gridDataModel.OnItemSpawned += HandleItemSpawned;
            _gridDataModel.OnCellsUnlocked += HandleCellsUnlocked; 
        }

        private void HandleItemPlaced(Vector2Int pos, IGridItem item)
        {
            TryProcessItemDiscovery(pos, item);
        }

        private void HandleItemSpawned(Vector2Int fromPos, Vector2Int toPos, IGridItem item)
        {
            TryProcessItemDiscovery(toPos, item);
        }

        private void HandleCellsUnlocked(IReadOnlyList<Vector2Int> unlockedCells)
        {
            foreach (var pos in unlockedCells)
            {
                IGridItem item = _gridDataModel.GetItemAt(pos);
                if (item == null) continue;
            
                _discoveryModifier.TryUnlockItem(item.Id, item.Level);
            }
        }

        private void TryProcessItemDiscovery(Vector2Int pos, IGridItem item)
        {
            if (item == null) return;

            if (_gridDataModel.IsCellLocked(pos)) 
            {
                return;
            }
            
            _discoveryModifier.TryUnlockItem(item.Id, item.Level);
        }

        public void Dispose()
        {
            if (_gridDataModel != null)
            {
                _gridDataModel.OnItemPlaced -= HandleItemPlaced;
                _gridDataModel.OnItemSpawned -= HandleItemSpawned;
                _gridDataModel.OnCellsUnlocked -= HandleCellsUnlocked;
            }
        }
    }
}