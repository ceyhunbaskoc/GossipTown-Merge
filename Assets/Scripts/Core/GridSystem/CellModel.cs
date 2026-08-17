using Core.SaveSystem;
using UnityEngine;

namespace Core.GridSystem
{
    public class CellModel
    {
        public Vector2Int Position { get; private set; }
        public IGridItem Item { get; private set; }
        public bool IsOccupied => Item != null;
        public bool IsLocked { get; private set; }

        public CellModel(Vector2Int position, bool isLocked = false)
        {
            Position = position;
            Item = null;
            IsLocked = isLocked;
        }

        public void SetItem(IGridItem item)
        {
            Item = item;
        }
        
        public void SetUnlocked() => IsLocked = false;

        public void Clear()
        {
            Item = null;
        }
        
        public CellSaveData GetSaveData()
        {
            CellSaveData data = new CellSaveData
            {
                Position = this.Position,
                IsLocked = this.IsLocked,
                HasItem = this.IsOccupied
            };

            if (this.IsOccupied && this.Item is ItemData itemData)
            {
                data.ItemData = itemData.GetSaveData();
            }

            return data;
        }
    }
}