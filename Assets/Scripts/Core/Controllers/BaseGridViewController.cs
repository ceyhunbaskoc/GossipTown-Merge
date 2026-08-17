using System;
using System.Collections.Generic;
using Core.Factories;
using Core.GridSystem;
using Core.Services;
using Core.Views;
using Data;
using UnityEngine;

namespace Core.Controllers
{
    public abstract class BaseGridViewController : MonoBehaviour, IDropRequestReceiver, IInteractRequestReceiver
    {
        [Header("Dependencies")]
        [SerializeField] protected MergeItemFactory _itemFactory;
        
        [Header("Visuals")]
        [SerializeField] protected GridVisualizer _gridVisualizer;
        
        protected float _cellSize;
        protected GridDataModel _gridModel;
        protected BoardSelectionService _selectionService;
        
        protected readonly Dictionary<IGridItem, IViewItem> _visualRegistry = new Dictionary<IGridItem, IViewItem>();
        protected Dictionary<Vector2Int, IViewCell> _cellRegistry;
        
        public event Action OnAnyUserInteraction;

        public virtual void Initialize(GridDataModel gridModel, int width, int height, float cellSize, BoardSelectionService selectionService)
        {
            _gridModel = gridModel;
            _cellSize = cellSize;
            _selectionService = selectionService;

            _cellRegistry = _gridVisualizer.DrawBoard(width, height, cellSize);

            _gridModel.OnItemPlaced += HandleItemPlaced;
            _gridModel.OnItemMoved += HandleItemMoved;
            _gridModel.OnCellCleared += HandleCellCleared;
            _gridModel.OnCellsUnlocked += HandleCellsUnlocked;
            _gridModel.OnItemSpawned += HandleItemSpawned;
            
            SyncInitialBoardState(width, height);
        }

        protected virtual void OnDestroy()
        {
            if (_gridModel != null)
            {
                _gridModel.OnItemPlaced -= HandleItemPlaced;
                _gridModel.OnItemMoved -= HandleItemMoved;
                _gridModel.OnCellCleared -= HandleCellCleared;
                _gridModel.OnCellsUnlocked -= HandleCellsUnlocked;
                _gridModel.OnItemSpawned -= HandleItemSpawned;
            }
        }

        private void SyncInitialBoardState(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (_cellRegistry.TryGetValue(pos, out IViewCell viewCell))
                    {
                        if (_gridModel.IsCellLocked(pos))
                        {
                            if (_gridModel.IsCellUnlockable(pos)) viewCell.SetState(CellVisualState.LockedUnlockable);
                            else viewCell.SetState(CellVisualState.LockedObscured);
                        }
                        else
                        {
                            viewCell.SetState(CellVisualState.Unlocked);
                        }
                    }
            
                    IGridItem existingItem = _gridModel.GetItemAt(pos);
                    if (existingItem != null) HandleItemPlaced(pos, existingItem); 
                }
            }
        }

        private void HandleItemPlaced(Vector2Int position, IGridItem itemData)
        {
            Vector3 worldPos = GridToWorldPosition(position);
            IViewItem newViewItem = _itemFactory.CreateItem(itemData, worldPos, this, this);
            newViewItem.CurrentGridPosition = position;
            
            bool isCellLocked = _gridModel.IsCellLocked(position);
            if (newViewItem is DraggableItem draggableItem)
            {
                draggableItem.SetLockedState(isCellLocked);
            }

            _visualRegistry.Add(itemData, newViewItem);
        }

        private void HandleCellCleared(Vector2Int position)
        {
            IGridItem itemToRemove = null;
            IViewItem viewToRemove = null;

            foreach (var kvp in _visualRegistry)
            {
                if (kvp.Value.CurrentGridPosition == position)
                {
                    itemToRemove = kvp.Key;
                    viewToRemove = kvp.Value;
                    break;
                }
            }

            if (itemToRemove != null && viewToRemove != null)
            {
                _visualRegistry.Remove(itemToRemove);
                _itemFactory.RecycleItem(viewToRemove);
            }
        }

        private void HandleCellsUnlocked(IReadOnlyList<Vector2Int> positions)
        {
            foreach (var cellPos in positions)
            {
                if (_cellRegistry.TryGetValue(cellPos, out IViewCell viewCell))
                {
                    viewCell.SetState(CellVisualState.Unlocked);
                }
        
                IGridItem unlockedItemData = _gridModel.GetItemAt(cellPos);
                if (unlockedItemData != null && _visualRegistry.TryGetValue(unlockedItemData, out IViewItem viewItem))
                {
                    if (viewItem is DraggableItem draggableItem) draggableItem.SetLockedState(false);
                }
            }

            Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
    
            foreach (var unlockedPos in positions)
            {
                foreach (var dir in directions)
                {
                    Vector2Int neighborPos = unlockedPos + dir;
            
                    if (_gridModel.IsValidBoundary(neighborPos) && _gridModel.IsCellLocked(neighborPos))
                    {
                        if (_cellRegistry.TryGetValue(neighborPos, out IViewCell neighborViewCell))
                        {
                            neighborViewCell.SetState(CellVisualState.LockedUnlockable);
                        }
                    }
                }
            }
        }

        private void HandleItemSpawned(Vector2Int fromPos, Vector2Int toPos, IGridItem itemData)
        {
            Vector3 startWorldPos = GridToWorldPosition(fromPos);
            Vector3 targetWorldPos = GridToWorldPosition(toPos);

            IViewItem newViewItem = _itemFactory.CreateItem(itemData, startWorldPos, this, this);
            newViewItem.CurrentGridPosition = toPos;

            _visualRegistry.Add(itemData, newViewItem);
            newViewItem.MoveWithArcTo(targetWorldPos); 
        }

        private void HandleItemMoved(Vector2Int fromPos, Vector2Int toPos, IGridItem itemData)
        {
            if (_visualRegistry.TryGetValue(itemData, out IViewItem viewItem))
            {
                Vector3 targetWorldPos = GridToWorldPosition(toPos);
                viewItem.MoveToWorldPosition(targetWorldPos);
                viewItem.CurrentGridPosition = toPos;
            }
        }

        public IViewItem ExtractVisualAt(Vector2Int position)
        {
            IGridItem itemData = _gridModel.GetItemAt(position);
            if (itemData != null && _visualRegistry.TryGetValue(itemData, out IViewItem viewItem))
            {
                _visualRegistry.Remove(itemData);
                return viewItem;
            }
            return null;
        }

        public IViewItem GetVisualAt(Vector2Int position)
        {
            IGridItem itemData = _gridModel.GetItemAt(position);
            if (itemData != null && _visualRegistry.TryGetValue(itemData, out IViewItem viewItem))
            {
                return viewItem;
            }
            return null;
        }

        protected Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 localPos = worldPosition - _gridVisualizer.GridOrigin;
            int x = Mathf.FloorToInt(localPos.x / _cellSize);
            int y = Mathf.FloorToInt(localPos.y / _cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            float halfCellOffset = _cellSize / 2f;
            return _gridVisualizer.GridOrigin + new Vector3(
                (gridPosition.x * _cellSize) + halfCellOffset, 
                (gridPosition.y * _cellSize) + halfCellOffset, 
                0f
            );
        }

        protected void TriggerInteractionEvent() => OnAnyUserInteraction?.Invoke();
        public abstract void OnItemDropRequested(IViewItem viewItem, Vector3 dropWorldPosition);
        public abstract void OnItemInteractRequested(Vector2Int gridPosition, IGridItem gridItem);
    }
}