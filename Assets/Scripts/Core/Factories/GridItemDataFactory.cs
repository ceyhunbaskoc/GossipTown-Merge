using Core.GridSystem;
using Data;

namespace Core.Factories
{
    public class GridItemDataFactory
    {
        private readonly ItemDatabaseSO _itemDatabase;

        public GridItemDataFactory(ItemDatabaseSO itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }

        public IGridItem CreateItemData(string id, int level)
        {
            BaseItemDefinitionSO def = _itemDatabase.GetItemDef(id);
            if (def == null) return null;

            return def.CreateRuntimeData(id, level);
        }
    }
}