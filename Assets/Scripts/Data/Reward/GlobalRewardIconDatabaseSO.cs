using System;
using UnityEngine;
using Data.Quests;

namespace Data.Reward
{
    [Serializable]
    public struct CategoryIconMapping
    {
        public RewardCategory Category;
        public Sprite CategoryIcon;
    }

    [CreateAssetMenu(fileName = "GlobalRewardIconDatabase", menuName = "GameData/Reward/GlobalRewardIconDatabase")]
    public class GlobalRewardIconDatabaseSO : ScriptableObject
    {
        [SerializeField] private CategoryIconMapping[] _categoryIcons;
        
        [SerializeField] private Sprite _fallbackIcon;
        
        [Header("Special Icons")]
        [field: SerializeField] public Sprite MedalIcon { get; private set; }

        public Sprite GetIconForCategory(RewardCategory category)
        {
            foreach (var mapping in _categoryIcons)
            {
                if (mapping.Category == category)
                {
                    return mapping.CategoryIcon;
                }
            }
            
            Debug.LogWarning($"[GlobalRewardIconDatabaseSO] Missing icon mapping for category: {category}");
            return _fallbackIcon;
        }
    }
}