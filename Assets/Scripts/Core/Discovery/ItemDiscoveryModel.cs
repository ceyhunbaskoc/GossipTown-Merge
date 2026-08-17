using System;
using System.Collections.Generic;
using System.Linq;
using Core.SaveSystem;

namespace Core.Discovery
{
    public class ItemDiscoveryModel : IReadOnlyItemDiscovery, IItemDiscoveryModifier
    {
        private readonly HashSet<(string itemId, int level)> _unlockedLevels = new HashSet<(string, int)>();
        
        private readonly Dictionary<string, int> _maxLevelsCache = new Dictionary<string, int>();
        
        public event Action<string,int> OnItemUnlocked;

        public void LoadSaveData(List<ItemDiscoverySaveData> savedData)
        {
            _unlockedLevels.Clear();
            _maxLevelsCache.Clear();

            if (savedData != null)
            {
                foreach (var data in savedData)
                {
                    InternalUnlock(data.ItemId, data.Level);
                }
            }
        }

        public List<ItemDiscoverySaveData> GetSaveData()
        {
            List<ItemDiscoverySaveData> saveData = new List<ItemDiscoverySaveData>();
            foreach (var unlock in _unlockedLevels)
            {
                saveData.Add(new ItemDiscoverySaveData
                {
                    ItemId = unlock.itemId,
                    Level = unlock.level
                });
            }
            return saveData;
        }

        public IReadOnlyList<string> GetUnlockedItemIds()
        {
            return _maxLevelsCache.Keys.ToList();
        }

        public int GetMaxUnlockedLevelFor(string itemId)
        {
            return _maxLevelsCache.TryGetValue(itemId, out int maxLevel) ? maxLevel : 0;
        }

        public bool IsItemUnlocked(string itemId, int level)
        {
            return _unlockedLevels.Contains((itemId, level));
        }

        public bool TryUnlockItem(string itemId, int level)
        {
            if (InternalUnlock(itemId, level))
            {
                OnItemUnlocked?.Invoke(itemId, level);
                return true;
            }
            return false;
        }

        private bool InternalUnlock(string itemId, int level)
        {
            if (!_unlockedLevels.Add((itemId, level))) return false;

            if (_maxLevelsCache.TryGetValue(itemId, out int currentMax))
            {
                if (level > currentMax) _maxLevelsCache[itemId] = level;
            }
            else
            {
                _maxLevelsCache.Add(itemId, level);
            }

            return true;
        }
        
        public IReadOnlyDictionary<string, int> UnlockedItems()
        {
            return _maxLevelsCache;
        }
    }
}