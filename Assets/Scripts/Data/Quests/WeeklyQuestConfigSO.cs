using System;
using System.Collections.Generic;
using Data.Roadmap;
using UnityEngine;

namespace Data.Quests
{
    [Serializable]
    public class DailyQuestGroup
    {
        public int DayIndex;
        public List<QuestDefinitionSO> Quests = new List<QuestDefinitionSO>();
        public List<QuestRewardConfig> DayCompletionRewards = new List<QuestRewardConfig>(); 
    }

    [CreateAssetMenu(fileName = "WeeklyQuestConfig", menuName = "GameData/Quests/WeeklyQuestConfig")]
    public class WeeklyQuestConfigSO : ScriptableObject
    {
        public string WeekId;
        public List<DailyQuestGroup> DailyGroups = new List<DailyQuestGroup>();
    }
}