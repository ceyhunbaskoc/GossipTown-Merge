using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "ItemDatabaseSO", menuName = "Scriptable Objects/ItemDatabase")]
    public class ItemDatabaseSO : ScriptableObject
    {
        [Header("Normal Items")]
        [field: SerializeField] public List<ItemDefinitionSO> NormalItems { get; private set; }
        
        [Header("Spawners")]
        [field: SerializeField] public List<SpawnerDefinitionSO> Spawners { get; private set; }
        
        [Header("Chests")]
        [field: SerializeField] public List<ChestDefinitionSO> Chests { get; private set; }
        [Header("Collectibles")]
        [field: SerializeField] public List<CollectibleItemDefinitionSO> Collectibles { get; private set; }

        private Dictionary<string, ItemDefinitionSO> _normalItemCache;
        private Dictionary<string, SpawnerDefinitionSO> _spawnerCache;
        private Dictionary<string, ChestDefinitionSO> _chestCache;
        private Dictionary<string, CollectibleItemDefinitionSO> _collectibleCache;
        
        public void InitializeDatabase()
        {
            _normalItemCache = new Dictionary<string, ItemDefinitionSO>();
            _spawnerCache = new Dictionary<string, SpawnerDefinitionSO>();
            _chestCache = new Dictionary<string, ChestDefinitionSO>();
            _collectibleCache = new Dictionary<string, CollectibleItemDefinitionSO>();

            foreach (var item in NormalItems)
            {
                if (item != null && !_normalItemCache.ContainsKey(item.Id))
                {
                    _normalItemCache.Add(item.Id, item);
                }
            }

            foreach (var spawner in Spawners)
            {
                if (spawner != null && !_spawnerCache.ContainsKey(spawner.Id))
                {
                    _spawnerCache.Add(spawner.Id, spawner);
                }
            }

            foreach (var chest in Chests)
            {
                if (chest != null && !_chestCache.ContainsKey(chest.Id))
                {
                    _chestCache.Add(chest.Id, chest);
                }
            }
            
            foreach (var collectible in Collectibles)
            {
                if (collectible != null && !_collectibleCache.ContainsKey(collectible.Id))
                {
                    _collectibleCache.Add(collectible.Id, collectible);
                }
            }
        }

        public BaseItemDefinitionSO GetItemDef(string id)
        {
            if (_normalItemCache == null || _spawnerCache == null)
            {
                Debug.LogError("Database not initialize!");
                return null;
            }

            if (_normalItemCache.TryGetValue(id, out var normalItem))
                return normalItem;

            if (_spawnerCache.TryGetValue(id, out var spawnerItem))
                return spawnerItem;
            
            if(_chestCache.TryGetValue(id, out var chestItem))
                return chestItem;
            
            if (_collectibleCache.TryGetValue(id, out var collectibleItem)) 
                return collectibleItem;

            Debug.LogWarning($"Item not found in database: ID={id}");
            return null;
        }

        public SpawnerDefinitionSO GetSpawnerDef(string id)
        {
            if (_spawnerCache != null && _spawnerCache.TryGetValue(id, out var spawnerItem))
            {
                return spawnerItem;
            }
            
            return null;
        }

        public ChestDefinitionSO GetChestDef(string id)
        {
            if (_chestCache != null && _chestCache.TryGetValue(id, out var chestItem))
            {
                return chestItem;
            }

            return null;
        }
        
        public CollectibleItemDefinitionSO GetCollectibleDef(string id)
        {
            if (_collectibleCache != null && _collectibleCache.TryGetValue(id, out var collectible)) return collectible;
            return null;
        }

        public List<BaseItemDefinitionSO> GetAllItems()
        {
            List<BaseItemDefinitionSO> allItems = new List<BaseItemDefinitionSO>();
            foreach (var item in NormalItems)
            {
                allItems.Add(item);
            }

            foreach (var spawner in Spawners)
            {
                allItems.Add(spawner);
            }

            foreach (var chest in Chests)
            {
                allItems.Add(chest);
            }
            
            foreach (var collectible in Collectibles)
            {
                allItems.Add(collectible);
            }

            return allItems;
        }
    }
}