using Core.GridSystem;
using Data;

namespace Core.Services
{
    public class LootGenerationService : ILootGenerationService
    {
        public ItemIdentifier GenerateLootForChest(ChestData chestData)
        {
            if (chestData == null || chestData.PossibleLoots == null || chestData.PossibleLoots.Count == 0)
            {
                return default; 
            }

            float totalWeight = 0f;
            foreach (var drop in chestData.PossibleLoots)
            {
                totalWeight += drop.Weight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var drop in chestData.PossibleLoots)
            {
                currentWeight += drop.Weight;
                if (randomValue <= currentWeight)
                {
                    return new ItemIdentifier(drop.ItemDefinition.Id, drop.Level);
                }
            }
            var lastDrop = chestData.PossibleLoots[^1];
            return new ItemIdentifier(lastDrop.ItemDefinition.Id, lastDrop.Level);
        }
    }
}