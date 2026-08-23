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
        
        private const float UNKNOWN_ITEM_BASE_WEIGHT = 30f; 

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

                    float weight = CalculateWeight(lvl, currentMaxDiscovered, countOnBoard);

                    weightedPool.Add(id, weight);
                    totalWeight += weight;
                }
            }

            if (weightedPool.Count == 0) return default;
            return SelectRandomWeighted(weightedPool, totalWeight);
        }

        private float CalculateWeight(int lvl, int currentMaxDiscovered, int countOnBoard)
        {
            float baseWeight;

            if (lvl > currentMaxDiscovered)
            {
                float friction = Mathf.Max(1f, lvl - 0.5f);
                baseWeight = UNKNOWN_ITEM_BASE_WEIGHT / friction; 
            }
            else if (lvl == currentMaxDiscovered)
            {
                baseWeight = 35f;
            }
            else
            {
                int distance = currentMaxDiscovered - lvl;
                baseWeight = Mathf.Max(5f, 25f - (distance * 10f)); 
            }

            float scarcityMultiplier = Mathf.Clamp(1f - (countOnBoard * 0.15f), 0.2f, 1f);

            return baseWeight * scarcityMultiplier;
        }

        private ItemIdentifier SelectRandomWeighted(Dictionary<ItemIdentifier, float> pool, float totalWeight)
        {
            float randomVal = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            foreach (var kvp in pool)
            {
                cumulativeWeight += kvp.Value;
                if (randomVal <= cumulativeWeight) return kvp.Key;
            }

            return pool.Keys.First();
        }
    }
}