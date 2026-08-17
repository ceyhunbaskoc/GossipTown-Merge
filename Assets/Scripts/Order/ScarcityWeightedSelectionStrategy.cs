using System.Collections.Generic;
using System.Linq;
using Core.Discovery;
using Core.GridSystem;
using Core.Services;
using Data;
using UnityEngine;

namespace Order
{
    public class ScarcityWeightedSelectionStrategy : IOrderSelectionStrategy
    {
        private const int MAX_LOOKBACK_OFFSET = 2;
        
        private const float UNKNOWN_ITEM_WEIGHT_MULTIPLIER = 0.05f; 

        public ItemIdentifier SelectItem(List<ItemDefinitionSO> availableItems, IReadOnlyItemDiscovery discovery, IBoardInventoryProvider boardInventory)
        {
            Dictionary<ItemIdentifier, float> weightedPool = new Dictionary<ItemIdentifier, float>();
            float totalWeight = 0f;

            foreach (var itemDef in availableItems)
            {
                int currentMaxDiscovered = discovery.GetMaxUnlockedLevelFor(itemDef.Id);
                
                int targetMaxLevel = Mathf.Min(currentMaxDiscovered + 1, itemDef.MaxLevel); 
                int targetMinLevel = Mathf.Max(1, currentMaxDiscovered - MAX_LOOKBACK_OFFSET);
                
                if (targetMinLevel > targetMaxLevel) targetMinLevel = targetMaxLevel;

                for (int lvl = targetMinLevel; lvl <= targetMaxLevel; lvl++)
                {
                    ItemIdentifier id = new ItemIdentifier(itemDef.Id, lvl);
                    int countOnBoard = boardInventory != null ? boardInventory.GetItemCountOnBoard(id) : 0;

                    float weight;

                    if (lvl > currentMaxDiscovered)
                    {
                        weight = 10f * UNKNOWN_ITEM_WEIGHT_MULTIPLIER;
                    }
                    else
                    {
                        weight = 10f + (countOnBoard * 10f);
                        if (lvl == currentMaxDiscovered) weight *= 0.5f; 
                    }

                    weightedPool.Add(id, weight);
                    totalWeight += weight;
                }
            }

            if (weightedPool.Count == 0) return default;

            float randomVal = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            foreach (var kvp in weightedPool)
            {
                cumulativeWeight += kvp.Value;
                if (randomVal <= cumulativeWeight) return kvp.Key;
            }

            return weightedPool.Keys.First();
        }
    }
}