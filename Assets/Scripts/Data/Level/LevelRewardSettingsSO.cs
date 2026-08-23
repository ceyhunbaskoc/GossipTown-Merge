using System;
using System.Collections.Generic;
using Data.Quests;
using UnityEngine;

namespace Data.Level
{
    [Serializable]
    public class LevelRewardSettings
    {
        public int Level;
        public List<QuestRewardConfig> RewardConfig;
    }
    
    [CreateAssetMenu(fileName = "LevelRewardSettings", menuName = "GameData/Progression/LevelRewardSettings")]
    
    public class LevelRewardSettingsSO : ScriptableObject
    {
        [field: SerializeField] public LevelRewardSettings[] LevelRewards { get; private set; }
        
        public IReadOnlyList<QuestRewardConfig> GetRewardForLevel(int level)
        {
            foreach (var reward in LevelRewards)
            {
                if (reward.Level == level)
                {
                    return reward.RewardConfig;
                }
            }
            return null;
        }
#if UNITY_EDITOR
        public void Editor_SetLevelRewards(LevelRewardSettings[] generatedRewards)
        {
            LevelRewards = generatedRewards;
        }
#endif
    }
}