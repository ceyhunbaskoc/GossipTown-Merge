using Core.AdService;
using Core.Controllers;
using Core.Economy;
using Core.Factories;
using Core.GridSystem;
using Data;
using UI.Chest;

namespace Core.Services
{
    public class SpawnerRestingService
    {
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly IEconomyModifier _economyModifier;
        private readonly IAdService _adService;
        
        public SpawnerRestingService(
            ItemDatabaseSO itemDatabase,
            IEconomyModifier economyModifier,
            IAdService adService
            )
        {
            _itemDatabase = itemDatabase;
            _economyModifier = economyModifier;
            _adService = adService;
        }

        public bool ForceSkipWithGems(SpawnerItemData spawnerItemData)
        {
            if (spawnerItemData == null || !spawnerItemData.IsInCooldown) return false;
    
            SpawnerDefinitionSO spawnerDef = GetSpawnerData(spawnerItemData);
            if (spawnerDef == null) return false;

            if (_economyModifier.TrySpendGems(spawnerDef.GetRestingSkipCost(spawnerItemData.Level)))
            {
                spawnerItemData.ForceComplete();
                return true;
            }
    
            return false;
        }

        public void ForceSkipWithAd(SpawnerItemData spawnerItemData)
        {
            _adService.ShowRewardedAd(
                onAdWatched: () =>
                {
                    spawnerItemData.ForceComplete();
                },
                onAdFailed: () =>
                {
                    //nothing
                });
        }

        public void ForceSkip(SpawnerItemData spawnerItemData)
        {
            spawnerItemData.ForceComplete();
        }
        
        private SpawnerDefinitionSO GetSpawnerData(SpawnerItemData chestItemData)
        {
            BaseItemDefinitionSO def = _itemDatabase.GetItemDef(chestItemData.Id);
            return def as SpawnerDefinitionSO;
        }
    }
}