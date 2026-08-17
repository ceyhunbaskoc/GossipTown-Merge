using System.Collections.Generic;
using Core.Discovery;
using Core.GridSystem;
using Core.Reward;
using Data;
using UnityEngine;

namespace Core.Services
{
    public class RewardSelectionService : IRewardSelectionService
    {
        private readonly GridDataModel _gridModel;
        private readonly RewardSelectionConfigSO _config;
        private readonly IReadOnlyItemDiscovery _itemDiscovery;
        
        private readonly ItemDatabaseSO _itemDatabase;

        public RewardSelectionService(
            GridDataModel gridModel, 
            RewardSelectionConfigSO config,  
            IReadOnlyItemDiscovery itemDiscovery,
            ItemDatabaseSO itemDatabase)
        {
            _gridModel = gridModel;
            _config = config;
            _itemDiscovery = itemDiscovery;
            _itemDatabase = itemDatabase;
        }

        public RewardPayload DetermineReward()
        {
            float totalWeight = _config.SpawnerUpgradeWeight + _config.ChestWeight + _config.NormalItemWeight;
            float randomVal = Random.Range(0f, totalWeight);

            if (randomVal <= _config.SpawnerUpgradeWeight)
            {
                return TryGetSpawnerUpgradeReward() ?? GetChestReward();
            }
            
            if (randomVal <= _config.SpawnerUpgradeWeight + _config.ChestWeight)
            {
                return GetChestReward();
            }

            return GetNormalItemReward();
        }

        public RewardPayload? TryGetSpawnerUpgradeReward()
        {
            List<SpawnerItemData> activeSpawners = new List<SpawnerItemData>();

            for (int x = 0; x < _gridModel.Width; x++)
            {
                for (int y = 0; y < _gridModel.Height; y++)
                {
                    IGridItem item = _gridModel.GetItemAt(new Vector2Int(x, y));
                    if (item is SpawnerItemData spawner)
                    {
                        activeSpawners.Add(spawner);
                    }
                }
            }

            if (activeSpawners.Count == 0) return null;

            SpawnerItemData selectedSpawner = activeSpawners[UnityEngine.Random.Range(0, activeSpawners.Count)];
            return new RewardPayload(selectedSpawner.Id, selectedSpawner.Level, 1);
        }

        public RewardPayload GetChestReward()
        {
            IReadOnlyList<string> unlockedIds = _itemDiscovery.GetUnlockedItemIds();
            List<string> unlockedChests = new List<string>();

            foreach (var chest in _itemDatabase.Chests)
            {
                unlockedChests.Add(chest.Id);
            }

            /*
            foreach (var id in unlockedIds)
            {
                BaseItemDefinitionSO def = _itemDatabase.GetItemDef(id);
                if (def is ChestDefinitionSO) 
                {
                    unlockedChests.Add(id);
                }
            }
            */
            if (unlockedChests.Count == 0)
            {
                return GetNormalItemReward();
            }

            string randomChestId = unlockedChests[Random.Range(0, unlockedChests.Count)];
            //TODO: The logic for selecting the chest level will be implemented later
            int randomLevel = Random.Range(1, 3);
            return new RewardPayload(randomChestId, randomLevel, 1);
        }

        private RewardPayload GetNormalItemReward()
        {
            IReadOnlyList<string> unlockedIds = _itemDiscovery.GetUnlockedItemIds();
            List<string> normalItems = new List<string>();

            foreach (var id in unlockedIds)
            {
                BaseItemDefinitionSO def = _itemDatabase.GetItemDef(id);
                if (!(def is ChestDefinitionSO) && !(def is SpawnerDefinitionSO))
                {
                    normalItems.Add(id);
                }
            }

            if (normalItems.Count == 0)
            {
                Debug.LogError("[RewardSelection] There are no unlocked normal items! Basic item are provided.");
                return new RewardPayload("coffee", 1, 1);
            }

            string randomItemId = normalItems[Random.Range(0, normalItems.Count)];
            
            int maxUnlockedLevel = _itemDiscovery.GetMaxUnlockedLevelFor(randomItemId);
            int randomLevel = Random.Range(1, maxUnlockedLevel + 1); 
            return new RewardPayload(randomItemId, randomLevel, 1);
        }
    }
}