using System.Collections.Generic;
using UnityEngine;

namespace Data.Roadmap
{
    [CreateAssetMenu(fileName = "NewBuildingDef", menuName = "Roadmap/Building Definition")]
    public class BuildingDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string BuildingId { get; private set; }
        [field: SerializeField] public string BuildingName { get; private set; }
        
        [field: SerializeField] public List<BuildingLevelData> Levels { get; private set; }

        public BuildingLevelData GetLevelData(int level)
        {
            if (level <= 0 || level > Levels.Count)
                return null;
                
            return Levels[level - 1];
        }

        public int MaxLevel => Levels.Count;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Levels == null || Levels.Count == 0) return;

            for (int i = 0; i < Levels.Count; i++)
            {
                var levelData = Levels[i];
                if (levelData.LevelRewards == null) continue;

                for (int j = 0; j < levelData.LevelRewards.Count; j++)
                {
                    var reward = levelData.LevelRewards[j];
                    if (reward == null) continue;

                    bool isCurrency = reward.Category == Data.Quests.RewardCategory.Gold || 
                                      reward.Category == Data.Quests.RewardCategory.Gem || 
                                      reward.Category == Data.Quests.RewardCategory.Energy;

                    if (!isCurrency && reward.RewardItem == null)
                    {
                        Debug.LogWarning($"[Veri Hatası] {BuildingName} - Seviye {i + 1}: {reward.Category} kategorisi seçilmiş ancak 'Reward Item' atanmamış! Lütfen bir eşya sürükleyin.", this);
                    }
                    
                    if (isCurrency && reward.RewardItem != null)
                    {
                        Debug.LogWarning($"[Tasarım Uyarısı] {BuildingName} - Seviye {i + 1}: Para birimleri ({reward.Category}) için 'Reward Item' atamanıza gerek yoktur. Referansı temizleyebilirsiniz.", this);
                    }
                }
            }
        }
#endif
    }
}