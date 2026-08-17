using Core.GridSystem;
using Data;

namespace Core.Rules
{
    public class StandardMergeValidator : IMergeValidator
    {
        
        private readonly ItemDatabaseSO _itemDatabase;

        public StandardMergeValidator(ItemDatabaseSO itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }
        public bool CanMerge(IGridItem item1, IGridItem item2)
        {
            if (item1 == null || item2 == null) return false;
            
            if (item1.Id != item2.Id) return false;
            if (item1.Level != item2.Level) return false;
            BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(item1.Id);

            if (item1.Level >= itemDef.MaxLevel)
            {
                return false;
            }
            return true;
        }
    }
}