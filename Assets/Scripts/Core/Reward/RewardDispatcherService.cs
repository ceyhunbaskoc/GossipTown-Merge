using Core.Economy;
using Core.GridSystem;
using Core.LevelSystem;
using Data.Quests;

namespace Core.Reward
{
    public class RewardDispatcherService
    {
        private readonly PendingRewardModel _pendingRewardModel;
        private readonly IEconomyModifier _economyModifier;
        private readonly ILevelModifier _levelModifier;

        public RewardDispatcherService(PendingRewardModel pendingRewardModel, IEconomyModifier economyModifier, ILevelModifier levelModifier)
        {
            _pendingRewardModel = pendingRewardModel;
            _economyModifier = economyModifier;
            _levelModifier = levelModifier;
        }

        public void DispatchReward(RewardPayload payload)
        {
            for (int i = 0; i < payload.Amount; i++)
            {
                ItemIdentifier rewardItem = new ItemIdentifier(payload.ItemId, payload.Level);
                _pendingRewardModel.AddReward(rewardItem);
            }
        }

        public void DispatchCurrencyReward(QuestRewardConfig rewardConfig)
        {
            switch (rewardConfig.Category)
            {
                case RewardCategory.Energy:
                    _economyModifier.AddEnergy(rewardConfig.Amount);
                    break;
                case RewardCategory.Gem:
                    _economyModifier.AddGem(rewardConfig.Amount);
                    break;
                case RewardCategory.Gold:
                    _economyModifier.AddGold(rewardConfig.Amount);
                    break;
                case RewardCategory.Experience:
                    _levelModifier.TryAddExperience(rewardConfig.Amount);
                    break;
            }
        }
    }
}