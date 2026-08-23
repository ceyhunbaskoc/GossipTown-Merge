using System;
using System.Collections.Generic;
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
        private readonly IEconomyModifier _economyModifier;
        private readonly RoadmapDatabaseSO _roadmapDatabase;

        private readonly Dictionary<string, Queue<(int Level, BuildingLevelData Data)>> _pendingRewardsQueue = new Dictionary<string, Queue<(int Level, BuildingLevelData Data)>>();
        public RoadmapRewardIntegrator(
            RoadmapProgressionService roadmapService, 
            RewardDispatcherService rewardService, 
            IEconomyModifier economyModifier, 
            RewardSelectionService rewardSelectionService,
            RoadmapDatabaseSO roadmapDatabase)
        {
            _roadmapService = roadmapService;
            _rewardService = rewardService;
            _economyModifier = economyModifier;
            _rewardSelectionService = rewardSelectionService;
            _roadmapDatabase = roadmapDatabase;
        
            _roadmapService.OnNodeUpgraded += HandleDomainUpgrade;

            RecoverLostRewards();
        }

        private void RecoverLostRewards()
        {
            if (_roadmapDatabase == null || _roadmapDatabase.AllNodes == null) return;

            foreach (var nodeDef in _roadmapDatabase.AllNodes)
            {
                int currentLevel = _roadmapService.GetNodeCurrentLevel(nodeDef.NodeId);
                int lastClaimedLevel = _roadmapService.GetLastClaimedRewardLevel(nodeDef.NodeId);

                if (currentLevel > lastClaimedLevel)
                {
                    for (int level = lastClaimedLevel + 1; level <= currentLevel; level++)
                    {
                        var levelData = nodeDef.Building.GetLevelData(level);
                        if (levelData != null)
                        {
                            ExecuteRewardDispatch(levelData);
                            _roadmapService.MarkRewardAsClaimed(nodeDef.NodeId, level);
                        }
                    }
                }
            }
        }

        private void HandleDomainUpgrade(MapNodeDefinitionSO nodeDef, BuildingLevelData levelData)
        {
            if (!_pendingRewardsQueue.ContainsKey(nodeDef.NodeId))
            {
                _pendingRewardsQueue[nodeDef.NodeId] = new Queue<(int, BuildingLevelData)>();
            }

            int snapshotLevel = _roadmapService.GetNodeCurrentLevel(nodeDef.NodeId);
            _pendingRewardsQueue[nodeDef.NodeId].Enqueue((snapshotLevel, levelData));
        }

        public void OnVisualUpgradeCompleted(string nodeId)
        {
            if (_pendingRewardsQueue.TryGetValue(nodeId, out var queue) && queue.Count > 0)
            {
                var payload = queue.Dequeue();
                int eventLevel = payload.Level;
                BuildingLevelData levelData = payload.Data;
                
                ExecuteRewardDispatch(levelData);
                
                _roadmapService.MarkRewardAsClaimed(nodeId, eventLevel);
            }
        }
        
        private void ExecuteRewardDispatch(BuildingLevelData levelData)
        {
            if (levelData.LevelRewards == null || levelData.LevelRewards.Count == 0) return;

            foreach(var reward in levelData.LevelRewards)
            {
                DispatchReward(reward);
            }
        }

        private void DispatchReward(BuildingRewardDefinition rewardDef)
        {
            switch (rewardDef.Category)
            {
                case RewardCategory.ItemSpawner:
                case RewardCategory.Chest:
                    RewardPayload payload = new RewardPayload(
                        itemId: rewardDef.RewardItem.Id,
                        level: rewardDef.Level,
                        amount: rewardDef.Amount);
                    _rewardService.DispatchReward(payload);
                    break;
                    
                default:
                    QuestRewardConfig currencyReward = new QuestRewardConfig
                        { Amount = rewardDef.Amount, Category = rewardDef.Category, ItemDefinition = null, Level = 0 };
                    _rewardService.DispatchCurrencyReward(currencyReward);
                    break;
            }
        }

        public void Dispose()
        {
            _roadmapService.OnNodeUpgraded -= HandleDomainUpgrade;
        }
    }
}