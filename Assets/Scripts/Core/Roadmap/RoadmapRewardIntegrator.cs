using System;
using Core.Economy;
using Core.Reward;
using Core.Services;
using Data.Quests;
using Data.Roadmap;
using UnityEngine;

namespace Core.Roadmap
{
    public class RoadmapRewardIntegrator : IDisposable
    {
        private readonly RoadmapProgressionService _roadmapService;
        private readonly RewardDispatcherService _rewardService;
        private readonly RewardSelectionService _rewardSelectionService;
        
        //TODO: Later on, something like a shared `dispatchCurrencyReward` function should be called. That function should also navigate to the location in the top bar using an animation.
        private readonly IEconomyModifier _economyModifier;

        public RoadmapRewardIntegrator(RoadmapProgressionService roadmapService, RewardDispatcherService rewardService, IEconomyModifier economyModifier, RewardSelectionService rewardSelectionService)
        {
            _roadmapService = roadmapService;
            _rewardService = rewardService;
            _economyModifier = economyModifier;
            _rewardSelectionService = rewardSelectionService;
        
            _roadmapService.OnNodeUpgraded += HandleRewardsOnUpgrade;
        }

        private void HandleRewardsOnUpgrade(MapNodeDefinitionSO nodeDef, BuildingLevelData levelData)
        {
            if (levelData.LevelRewards != null && levelData.LevelRewards.Count > 0)
            {
                foreach(var reward in levelData.LevelRewards)
                {
                    DispatchReward(reward);
                }
            }
        }
        
        private void DispatchReward(BuildingRewardDefinition rewardDef)
        {
            switch (rewardDef.Category)
            {
                case RewardCategory.ItemSpawner:
                    RewardPayload itemRewardPayload = new RewardPayload(
                        itemId: rewardDef.RewardItem.Id,
                        level: rewardDef.Level,
                        amount: rewardDef.Amount);
                    _rewardService.DispatchReward(itemRewardPayload);
                    /*RewardPayload? spawnerReward = _rewardSelectionService.TryGetSpawnerUpgradeReward();
                    if (spawnerReward.HasValue)
                    {
                        _rewardService.DispatchReward(spawnerReward.Value);
                    }
                    else
                    {
                        Debug.LogError("Item spawner reward can't dispatch");
                        RewardPayload chestReward = _rewardSelectionService.GetChestReward();
                        _rewardService.DispatchReward(chestReward);
                    }*/
                    break;
                case RewardCategory.Chest:
                    RewardPayload rewardPayload = new RewardPayload(
                        itemId: rewardDef.RewardItem.Id,
                        level: rewardDef.Level,
                        amount: rewardDef.Amount);
                    _rewardService.DispatchReward(rewardPayload);
                    break;
                    
                default:
                    _dispatchCurrencyReward(rewardDef);
                    break;
            }
        }

        private void _dispatchCurrencyReward(BuildingRewardDefinition rewardDef)
        {
            switch (rewardDef.Category)
            {
                case RewardCategory.Energy:
                    _economyModifier.AddEnergy(rewardDef.Amount);
                    break;
                case RewardCategory.Gem:
                    _economyModifier.AddGem(rewardDef.Amount);
                    break;
                case RewardCategory.Gold:
                    _economyModifier.AddGold(rewardDef.Amount);
                    break;
            }
        }

        public void Dispose()
        {
            _roadmapService.OnNodeUpgraded -= HandleRewardsOnUpgrade;
        }
    }
}