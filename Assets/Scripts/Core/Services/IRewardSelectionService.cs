using Core.Reward;

namespace Core.Services
{
    public interface IRewardSelectionService
    {
        RewardPayload DetermineReward();
        RewardPayload GetNormalItemReward();
    }
}