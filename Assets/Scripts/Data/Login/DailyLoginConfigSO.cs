using System;
using System.Collections.Generic;
using Core.Reward;
using Data.Quests;
using UnityEngine;

namespace Data.Login
{
    [Serializable]
    public class LoginRewardItem
    {
        public QuestRewardConfig RewardConfig;
        public int MedalCount;
    }
    [CreateAssetMenu(fileName = "DailyLoginConfig", menuName = "GameData/Login/DailyLoginConfig")]
    public class DailyLoginConfigSO : ScriptableObject
    {
        [field: SerializeField] public int UILoopDays { get; private set; } = 7;
        
        [field: SerializeField] public List<LoginRewardItem> ContinuousRewards { get; private set; }
        
        public LoginRewardItem GetRewardForTotalDays(int totalDays)
        {
            if (ContinuousRewards == null || ContinuousRewards.Count == 0) return null;
            
            int safeIndex = totalDays % ContinuousRewards.Count;
            
            return ContinuousRewards[safeIndex];
        }
    }
}