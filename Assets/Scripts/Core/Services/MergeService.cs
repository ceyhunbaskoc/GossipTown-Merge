using System;
using System.Collections.Generic;
using Core.Factories;
using Core.GridSystem;
using Core.Rules;
using Data;
using UnityEngine;

namespace Core.Services
{
    public class MergeService : IMergeService
    {
        private readonly IGridModifier _gridModifier;
        private readonly IReadOnlyGrid _readOnlyGrid;
        private readonly IMergeValidator _mergeValidator;
        private readonly GridItemDataFactory _dataFactory;
        
        public event Action<IGridItem> OnItemMerged;
        
        private Vector2Int[] _directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        public MergeService(IGridModifier gridModifier, IReadOnlyGrid readOnlyGrid, IMergeValidator mergeValidator, GridItemDataFactory dataFactory)
        {
            _gridModifier = gridModifier;
            _readOnlyGrid = readOnlyGrid;
            _mergeValidator = mergeValidator;
            _dataFactory = dataFactory;
        }
        
        public bool TryProcessMerge(Vector2Int fromPosition, Vector2Int toPosition)
        {
            IGridItem movingItem = _readOnlyGrid.GetItemAt(fromPosition);
            IGridItem targetItem = _readOnlyGrid.GetItemAt(toPosition);

            if (movingItem == null || targetItem == null) return false;
            if (_readOnlyGrid.IsCellLocked(fromPosition)) return false;
            if (!_mergeValidator.CanMerge(movingItem, targetItem))
            {
                return false; 
            }

            if (_readOnlyGrid.IsCellLocked(toPosition))
            {
                if (!_readOnlyGrid.IsCellUnlockable(toPosition))
                {
                    return false;
                }
        
                _gridModifier.TryUnlockLockedCells(toPosition);
            }
            
            _gridModifier.TryClearCell(fromPosition);
            _gridModifier.TryClearCell(toPosition);
            
            int newLevel = movingItem.Level + 1;
            IGridItem mergedItemData = _dataFactory.CreateItemData(movingItem.Id, newLevel);
            _gridModifier.TryPlaceObject(toPosition, mergedItemData);
            OnItemMerged?.Invoke(mergedItemData);
                
            return true;
        }
    }
}