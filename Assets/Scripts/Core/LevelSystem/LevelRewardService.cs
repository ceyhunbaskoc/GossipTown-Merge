using System;
using Core.Reward;
using Data.Level;
using Data.Quests;

namespace Core.LevelSystem
{
    public class LevelRewardService : IDisposable
    {
        private readonly IReadOnlyLevel _readOnlyLevel;
        private readonly LevelRewardSettingsSO _levelRewardSettings;
        private readonly RewardDispatcherService _rewardDispatcherService;

        public LevelRewardService(IReadOnlyLevel readOnlyLevel, LevelRewardSettingsSO levelRewardSettings, RewardDispatcherService rewardDispatcherService)
        {
            _readOnlyLevel = readOnlyLevel;
            _levelRewardSettings = levelRewardSettings;
            _rewardDispatcherService = rewardDispatcherService;
            
            _readOnlyLevel.OnLevelChanged += HandleLevelChanged;
        }
        
        private void HandleLevelChanged(int newLevel)
        {
            var rewardConfig = _levelRewardSettings.GetRewardForLevel(newLevel);
            if (rewardConfig != null)
            {
                foreach (var reward in rewardConfig)
                {
                    if (reward.Category == RewardCategory.Chest || reward.Category == RewardCategory.ItemSpawner)
                    {
                        RewardPayload payload = reward.ToPayload();
                    
                        if (payload.IsValid)
                        {
                            _rewardDispatcherService.DispatchReward(payload);
                        }
                    }
                    else
                    {
                        _rewardDispatcherService.DispatchCurrencyReward(reward);
                    }
                }
            }
        }
        
        public void Dispose()
        {
            _readOnlyLevel.OnLevelChanged -= HandleLevelChanged;
        }
    }
}