using System;
using Core.GridSystem;
using Data;
using UnityEngine;

namespace Core.Services
{
    public class BoardSelectionService : IDisposable
    {
        private readonly GridDataModel _gridModel;
        private readonly ItemDatabaseSO _itemDatabase;

        public event Action<IGridItem, BaseItemDefinitionSO> OnItemSelected;
        public event Action OnSelectionCleared;
        public event Action<bool> OnVisualsSuspended; 
        public Vector2Int? CurrentSelectedPosition { get; private set; }
        public bool IsVisualsSuspended { get; private set; } 

        public BoardSelectionService(GridDataModel gridModel, ItemDatabaseSO itemDatabase)
        {
            _gridModel = gridModel;
            _itemDatabase = itemDatabase;

            _gridModel.OnItemMoved += HandleItemMoved;
            _gridModel.OnCellCleared += HandleCellCleared;
        }

        public void SelectItemAt(Vector2Int position)
        {
            IGridItem item = _gridModel.GetItemAt(position);
            if (item != null)
            {
                CurrentSelectedPosition = position;
                BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(item.Id);
                OnItemSelected?.Invoke(item, itemDef);
            }
            else
            {
                ClearSelection();
            }
        }

        public void ClearSelection()
        {
            CurrentSelectedPosition = null;
            OnSelectionCleared?.Invoke();
        }
        
        public void SetVisualsSuspended(bool isSuspended)
        {
            IsVisualsSuspended = isSuspended;
            OnVisualsSuspended?.Invoke(isSuspended);
        }

        private void HandleItemMoved(Vector2Int fromPos, Vector2Int toPos, IGridItem item)
        {
            if (CurrentSelectedPosition == fromPos)
            {
                SelectItemAt(toPos);
            }
        }

        private void HandleCellCleared(Vector2Int position)
        {
            if (CurrentSelectedPosition == position)
            {
                ClearSelection();
            }
        }

        public void Dispose()
        {
            _gridModel.OnItemMoved -= HandleItemMoved;
            _gridModel.OnCellCleared -= HandleCellCleared;
        }
    }
}