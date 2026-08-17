using System;
using System.Collections.Generic;
using Core.GridSystem;
using Core.SaveSystem;

namespace Core.Reward
{
    public class PendingRewardModel
    {
        public event Action<ItemIdentifier> OnRewardClaimed;
        public event Action<ItemIdentifier> OnRewardAdded;
        
        private Queue<ItemIdentifier> _currentRewards = new Queue<ItemIdentifier>();
        
        public int PendingCount => _currentRewards.Count;
        
        public void AddReward(ItemIdentifier reward)
        {
            _currentRewards.Enqueue(reward);
            OnRewardAdded?.Invoke(reward);
        }

        public bool TryClaimNextReward(out ItemIdentifier claimedReward)
        {
            if(_currentRewards.Count>0)
            {
                claimedReward = _currentRewards.Dequeue();
                OnRewardClaimed?.Invoke(claimedReward);
                return true;
            }
            claimedReward = default;
            return false;
        }
        
        public bool TryPeekNextReward(out ItemIdentifier nextReward)
        {
            if (_currentRewards.Count > 0)
            {
                nextReward = _currentRewards.Peek();
                return true;
            }
            nextReward = default;
            return false;
        }

        public List<RewardSaveData> GetSaveData()
        {
            List<RewardSaveData> saveDataList = new List<RewardSaveData>();
            foreach (var reward in _currentRewards)
            {
                saveDataList.Add(new RewardSaveData
                {
                    ItemId = reward.Id,
                    Level = reward.Level
                });
            }
            return saveDataList;
        }
        
        public void LoadSaveData(List<RewardSaveData> saveDataList)
        {
            _currentRewards.Clear();
            foreach (var rewardData in saveDataList)
            {
                _currentRewards.Enqueue(new ItemIdentifier(rewardData.ItemId, rewardData.Level));
            }
        }
    }
}