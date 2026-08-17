using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data.Quests
{
    [CreateAssetMenu(fileName = "WeeklyQuestDatabase", menuName = "GameData/Quests/WeeklyQuestDatabase")]
    public class WeeklyQuestDatabaseSO : ScriptableObject
    {
        [field: SerializeField] public List<WeeklyQuestConfigSO> AllWeeks { get; private set; } = new List<WeeklyQuestConfigSO>();

        public WeeklyQuestConfigSO GetWeekById(string weekId)
        {
            return AllWeeks.FirstOrDefault(week => week.WeekId == weekId);
        }
        public WeeklyQuestConfigSO GetDefaultStarterWeek()
        {
            if (AllWeeks.Count == 0)
            {
                Debug.LogError("[WeeklyQuestDatabase] Database is empty! Please assign weeks in the Inspector.");
                return null;
            }
            
            return AllWeeks[0];
        }
    }
}