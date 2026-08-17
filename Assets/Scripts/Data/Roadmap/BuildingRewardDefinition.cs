using System;
using Data.Quests;
using UnityEngine;

namespace Data.Roadmap
{
    
    [Serializable]
    public class BuildingRewardDefinition
    {
        public RewardCategory Category;
        [Tooltip("If “item spawner” is selected as the enum, this field should remain blank")]
        public BaseItemDefinitionSO RewardItem;
        public int Level = 1;
        public int Amount = 1;
    }
}