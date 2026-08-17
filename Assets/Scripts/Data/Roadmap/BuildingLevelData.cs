using System;
using System.Collections.Generic;
using Data.Story;
using UnityEngine;

namespace Data.Roadmap
{
    [Serializable]
    public class BuildingLevelData
    {
        public Sprite LevelSprite;
        public int UpgradeCost;
        
        public List<BuildingRewardDefinition> LevelRewards = new List<BuildingRewardDefinition>();
    }
}