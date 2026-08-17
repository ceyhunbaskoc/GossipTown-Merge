using System;
using Core.Milestones;
using Core.Reward;
using Core.SaveSystem;
using Core.Services; 
using Data.Login;
using Data.Quests;
using UnityEngine;

namespace Core.Login
{
    public class DailyLoginService
    {
        public event Action<int> OnRewardClaimed;
        public event Action OnNewDayAvailable;

        private readonly DailyLoginConfigSO _config;
        private readonly RewardDispatcherService _rewardDispatcher;
        private readonly ITimeManager _timeManager;
        private readonly MilestoneService _milestoneService;
        private readonly RewardSelectionService _rewardSelectionService;

        private DailyLoginSaveData _saveData;
        private bool _isRewardAvailableToday;

        public bool IsRewardAvailableToday => _isRewardAvailableToday;
        public int TotalClaimedDays => _saveData != null ? _saveData.TotalClaimedDays : 0;

        public DailyLoginService(
            DailyLoginConfigSO config,
            RewardDispatcherService rewardDispatcher,
            ITimeManager timeManager,
            DailyLoginSaveData saveData,
            MilestoneService milestoneService,
            RewardSelectionService rewardSelectionService)
        {
            _config = config;
            _rewardDispatcher = rewardDispatcher;
            _timeManager = timeManager;
            _milestoneService = milestoneService;
            _rewardSelectionService = rewardSelectionService;
            
            LoadSaveData(saveData);
        }

        private void LoadSaveData(DailyLoginSaveData saveData)
        {
            _saveData = saveData ?? new DailyLoginSaveData();
            EvaluateDailyStatus();
        }

        private void EvaluateDailyStatus()
        {
            if (_saveData.LastClaimTimestampTicks == 0)
            {
                _isRewardAvailableToday = true;
                OnNewDayAvailable?.Invoke();
                return;
            }

            DateTime lastClaimTime = new DateTime(_saveData.LastClaimTimestampTicks);
            DateTime currentTime = new DateTime(_timeManager.CurrentTimeTicks);

            int daysPassed = (currentTime.Date - lastClaimTime.Date).Days;

            if (daysPassed > 0)
            {
                _isRewardAvailableToday = true;
                OnNewDayAvailable?.Invoke();
            }
            else
            {
                _isRewardAvailableToday = false;
            }
        }

        public void ClaimTodayReward()
        {
            if (!_isRewardAvailableToday)
            {
                Debug.LogWarning("[DailyLoginService] Illegal claim attempt.");
                return;
            }

            var rewardConfig = _config.GetRewardForTotalDays(_saveData.TotalClaimedDays);
            
            if (rewardConfig != null)
            {
                switch (rewardConfig.RewardConfig.Category)
                {
                    case RewardCategory.Chest:
                    {
                        RewardPayload payload = rewardConfig.RewardConfig.ToPayload();
                        if (payload.IsValid)
                        {
                            _rewardDispatcher.DispatchReward(payload);
                        }

                        break;
                    }
                    case RewardCategory.ItemSpawner:
                    {
                        RewardPayload? payload = _rewardSelectionService.TryGetSpawnerUpgradeReward();
                        if (payload.HasValue)
                        {
                            _rewardDispatcher.DispatchReward(payload.Value);
                        }

                        break;
                    }
                    default:
                        _rewardDispatcher.DispatchCurrencyReward(rewardConfig.RewardConfig);
                        break;
                }
                _milestoneService.AddMedals(rewardConfig.MedalCount);
            }

            _saveData.TotalClaimedDays++;
            _saveData.LastClaimTimestampTicks = _timeManager.CurrentTimeTicks;
            _isRewardAvailableToday = false;

            OnRewardClaimed?.Invoke(_saveData.TotalClaimedDays);
        }

        public DailyLoginSaveData GetSaveData()
        {
            return _saveData;
        }
        
        public void AutoClaimPendingMilestones()
        {
            if (_milestoneService != null)
            {
                _milestoneService.ClaimAllReadyTiers();
            }
        }
    }
}