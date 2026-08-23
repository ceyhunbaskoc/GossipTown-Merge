using Core.GridSystem;
using Data;
using Core.SaveSystem;

namespace Core.Factories
{
    public class GridItemDataFactory
    {
        private readonly ItemDatabaseSO _itemDatabase;

        public GridItemDataFactory(ItemDatabaseSO itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }

        public IGridItem CreateItemDataFromSave(ItemSaveData saveData)
        {
            BaseItemDefinitionSO def = _itemDatabase.GetItemDef(saveData.Id);
            if (def == null) return null;

            IGridItem runtimeItem = def.CreateRuntimeData(saveData.Id, saveData.Level);

            if (runtimeItem is ICustomSaveableItem customItem && !string.IsNullOrEmpty(saveData.CustomDataJson))
            {
                customItem.LoadCustomStateFromJson(saveData.CustomDataJson);
            }

            return runtimeItem;
        }

        public IGridItem CreateItemData(string id, int level)
        {
            BaseItemDefinitionSO def = _itemDatabase.GetItemDef(id);
            if (def == null) return null;
            return def.CreateRuntimeData(id, level);
        }
    }
}