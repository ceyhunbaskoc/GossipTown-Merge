using System.Collections.Generic;
using Core.GridSystem;
using Core.Reward;
using Data; // ItemIdentifier için
using UnityEngine;

namespace Core.Services
{
    public class MergeHintService
    {
        private readonly GridDataModel _gridModel;
        private readonly ItemDatabaseSO _itemDatabase;
        
        public MergeHintService(GridDataModel gridModel, ItemDatabaseSO itemDatabase)
        {
            _gridModel = gridModel;
            _itemDatabase = itemDatabase;
        }

        public bool TryGetMergeablePair(out Vector2Int pos1, out Vector2Int pos2)
        {
            pos1 = default;
            pos2 = default;

            Dictionary<ItemIdentifier, Vector2Int> seenItems = new Dictionary<ItemIdentifier, Vector2Int>();

            for (int x = 0; x < _gridModel.Width; x++)
            {
                for (int y = 0; y < _gridModel.Height; y++)
                {
                    Vector2Int currentPos = new Vector2Int(x, y);
                    
                    if (_gridModel.IsCellLocked(currentPos)) continue;

                    IGridItem item = _gridModel.GetItemAt(currentPos);
                    if (item == null) continue;

                    BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(item.Id);
                    if(itemDef == null || item.Level >= itemDef.MaxLevel) continue;

                    ItemIdentifier identifier = new ItemIdentifier(item.Id, item.Level);

                    if (seenItems.TryGetValue(identifier, out Vector2Int previousPos))
                    {
                        pos1 = previousPos;
                        pos2 = currentPos;
                        return true;
                    }

                    seenItems[identifier] = currentPos;
                }
            }

            return false;
        }
    }
}