using System;
using Core.Economy;
using Core.GridSystem;
using Core.Quests;
using Core.Services;
using Data.Quests;
using Data.Roadmap;
using Order;

namespace Core.Orchestration
{
    public class QuestEventIntegrator : IDisposable
    {
        private readonly WeeklyQuestService _questService;
        private readonly RoadmapProgressionService _roadmapService;
        private readonly IMergeService _mergeService;
        private readonly IReadOnlyOrder _orderDataModel;
        private readonly IReadOnlyEconomyModel _economyModel;
        
        // TODO: İleride IEconomyModifier, IMergeService gibi diğer sistemler de buraya eklenebilir.

        public QuestEventIntegrator(
            WeeklyQuestService questService, 
            RoadmapProgressionService roadmapService,
            IMergeService mergeService,
            IReadOnlyOrder orderDataModel,
            IReadOnlyEconomyModel economyModel)
        {
            _questService = questService;
            _roadmapService = roadmapService;
            _mergeService = mergeService;
            _orderDataModel = orderDataModel;
            _economyModel = economyModel;

            _roadmapService.OnNodeUpgraded += HandleBuildingUpgraded;
            _mergeService.OnItemMerged += HandleItemMerged;
            _orderDataModel.OnOrderCompleted += (ordeModel) => HandleOrderComplete();
            _economyModel.OnGoldSpend += HandleGoldSpent;
        }
        private void HandleBuildingUpgraded(MapNodeDefinitionSO nodeDef, BuildingLevelData nextLevelData)
        {
            _questService.ProcessAction(QuestType.UpgradeBuilding, 1, nodeDef.NodeId);
        }

        private void HandleItemMerged(IGridItem mergedItem)
        {
            _questService.ProcessAction(QuestType.MergeItem, 1, mergedItem.Id);
        }

        private void HandleOrderComplete()
        {
            _questService.ProcessAction(QuestType.CompleteOrder, 1);
        }

        private void HandleGoldSpent(int amount)
        {
            _questService.ProcessAction(QuestType.SpendGold, amount);
        }

        public void Dispose()
        {
            if (_roadmapService != null)
            {
                _roadmapService.OnNodeUpgraded -= HandleBuildingUpgraded;
            }
            if (_mergeService != null)
            {
                _mergeService.OnItemMerged -= HandleItemMerged;
            }
            _economyModel.OnGoldSpend -= HandleGoldSpent;
        }
    }
}