using UnityEngine;

namespace Core.Achievements
{
    public struct AchievementSlotViewModel
    {
        public string ItemId;
        public int Level;
        public Sprite Icon;
        public AchievementState State;
        public int RewardAmount;
    }
}