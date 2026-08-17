using System;
using System.Collections.Generic;
using Core.SaveSystem;

namespace Core.Achievements
{
    public enum AchievementState
    {
        Locked,           
        UnlockedUnclaimed,
        Claimed           
    }

    public class AchievementRewardModel
    {
        private readonly HashSet<(string itemId, int level)> _claimedRewards = new HashSet<(string, int)>();
        
        public event Action OnRewardClaimed;

        public void LoadSaveData(List<ClaimedAchievementSaveData> savedData)
        {
            _claimedRewards.Clear();
            if (savedData != null)
            {
                foreach (var data in savedData)
                {
                    _claimedRewards.Add((data.ItemId, data.Level));
                }
            }
        }

        public List<ClaimedAchievementSaveData> GetSaveData()
        {
            List<ClaimedAchievementSaveData> saveData = new List<ClaimedAchievementSaveData>();
            foreach (var reward in _claimedRewards)
            {
                saveData.Add(new ClaimedAchievementSaveData
                {
                    ItemId = reward.itemId,
                    Level = reward.level
                });
            }
            return saveData;
        }

        public bool IsRewardClaimed(string itemId, int level)
        {
            return _claimedRewards.Contains((itemId, level));
        }

        public void MarkRewardClaimed(string itemId, int level)
        {
            if (_claimedRewards.Add((itemId, level)))
            {
                OnRewardClaimed?.Invoke();
            }
        }
    }
}