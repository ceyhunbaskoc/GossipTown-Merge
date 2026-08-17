using System;
using System.Collections.Generic;
using Core.Reward;
using Core.SaveSystem;
using Data.Milestones;
using Data.Quests;
using UnityEngine;

namespace Core.Milestones
{
    public class MilestoneService
    {
        public event Action<int, int> OnMedalCountChanged;
        public event Action<MilestoneTier> OnTierUnlocked;
        public event Action<MilestoneTier> OnTierClaimed;

        private readonly MilestoneConfigSO _config;
        private readonly RewardDispatcherService _rewardDispatcher;
        
        private MilestoneSaveData _saveData;
        private readonly HashSet<string> _claimedTiersMap;

        public int CurrentMedals => _saveData.CurrentMedals;
        public int MaxMedals => _config.MaxMedals;
        public IEnumerable<MilestoneTier> GetTiers() => _config.Tiers;

        public MilestoneService(
            MilestoneConfigSO config, 
            RewardDispatcherService rewardDispatcher, 
            MilestoneSaveData saveData)
        {
            _config = config;
            _rewardDispatcher = rewardDispatcher;
            
            _saveData = saveData ?? new MilestoneSaveData();
            _claimedTiersMap = new HashSet<string>(_saveData.ClaimedTierIds);
        }

        public void AddMedals(int amount)
        {
            if (amount <= 0) return;

            _saveData.CurrentMedals += amount;
            if (_saveData.CurrentMedals > _config.MaxMedals)
            {
                _saveData.CurrentMedals = _config.MaxMedals;
            }

            OnMedalCountChanged?.Invoke(_saveData.CurrentMedals, _config.MaxMedals);

            EvaluateUnlockedTiers();
        }

        private void EvaluateUnlockedTiers()
        {
            foreach (var tier in _config.Tiers)
            {
                if (!_claimedTiersMap.Contains(tier.TierId) && _saveData.CurrentMedals >= tier.RequiredMedals)
                {
                    OnTierUnlocked?.Invoke(tier);
                }
            }
        }

        public void ClaimMilestone(string tierId)
        {
            if (_claimedTiersMap.Contains(tierId))
            {
                Debug.LogWarning($"[MilestoneService] Tier {tierId} zaten alınmış.");
                return;
            }

            var tierConfig = _config.Tiers.Find(t => t.TierId == tierId);
            
            if (tierConfig == null || _saveData.CurrentMedals < tierConfig.RequiredMedals)
            {
                Debug.LogWarning($"[MilestoneService] Tier {tierId} alınmaya uygun değil.");
                return;
            }

            switch (tierConfig.Reward.Category)
            {
                case RewardCategory.Chest:
                case RewardCategory.ItemSpawner:
                    RewardPayload payload = tierConfig.Reward.ToPayload();
                    if (payload.IsValid)
                    {
                        _rewardDispatcher.DispatchReward(payload);
                    }

                    break;
                default:
                    _rewardDispatcher.DispatchCurrencyReward(tierConfig.Reward);
                    break;
            }
            
            _saveData.ClaimedTierIds.Add(tierId);
            _claimedTiersMap.Add(tierId);

            OnTierClaimed?.Invoke(tierConfig);
        }

        public bool IsTierClaimed(string tierId)
        {
            return _claimedTiersMap.Contains(tierId);
        }
        
        public MilestoneSaveData GetSaveData()
        {
            return _saveData;
        }
        
        public void ClaimAllReadyTiers()
        {
            foreach (var tier in _config.Tiers)
            {
                if (!IsTierClaimed(tier.TierId) && _saveData.CurrentMedals >= tier.RequiredMedals)
                {
                    ClaimMilestone(tier.TierId);
                }
            }
        }
    }
}