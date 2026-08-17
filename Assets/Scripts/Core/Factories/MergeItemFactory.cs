using Core.GridSystem;
using Core.PoolSystem;
using Core.Rules;
using Data;
using UnityEngine;

namespace Core.Factories
{
    public class MergeItemFactory : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        
        private IObjectPool _objectPool;
        private IMergeValidator _mergeValidator;
        
        public void Initialize(IObjectPool objectPool, IMergeValidator mergeValidator)
        {
            _objectPool = objectPool;
            _mergeValidator = mergeValidator;
        }

        public DraggableItem CreateItem(IGridItem itemData, Vector3 position, IDropRequestReceiver dropReceiver, IInteractRequestReceiver interactReceiver)
        {
            GameObject spawnedObj = _objectPool.Spawn(PoolObjectType.DraggableItem, position, Quaternion.identity);
            if (spawnedObj.TryGetComponent<DraggableItem>(out DraggableItem newItem))
            {
                BaseItemDefinitionSO definition = _itemDatabase.GetItemDef(itemData.Id);
                Sprite icon = definition.GetIcon(itemData.Level);
                newItem.Initialize(itemData, dropReceiver, interactReceiver, _mergeValidator, icon);
                return newItem;
            }

            return null;
        }
        
        public void RecycleItem(IViewItem viewItem)
        {
            if (viewItem is MonoBehaviour viewBehaviour)
            {
                _objectPool.Despawn(PoolObjectType.DraggableItem, viewBehaviour.gameObject);
            }
        }
    }
}