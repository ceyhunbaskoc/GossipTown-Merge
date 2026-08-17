using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.GridSystem
{
    public interface IGridModifier
    {
        public event Action<Vector2Int, Vector2Int, IGridItem> OnItemSpawned;
        public event Action<Vector2Int, IGridItem> OnItemPlaced;
        public event Action<Vector2Int, Vector2Int, IGridItem> OnItemMoved;
        public event Action<Vector2Int> OnCellCleared;
        bool TrySpawnObject(Vector2Int fromPosition, Vector2Int toPosition, IGridItem item);
        bool TryPlaceObject(Vector2Int position, IGridItem item);
        bool TryUnlockLockedCells(Vector2Int position);
        bool TryMoveObject(Vector2Int from, Vector2Int to);
        bool TryClearCell(Vector2Int position);
    }
}