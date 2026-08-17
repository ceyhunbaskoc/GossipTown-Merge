using System;
using System.Collections.Generic;
using Data.Quests;
using UnityEngine;

namespace Data.Milestones
{
    [Serializable]
    public class MilestoneTier
    {
        public string TierId;
        public int RequiredMedals;
        public QuestRewardConfig Reward;
    }

    [CreateAssetMenu(fileName = "MilestoneConfig", menuName = "GameData/Milestones/MilestoneConfig")]
    public class MilestoneConfigSO : ScriptableObject
    {
        public List<MilestoneTier> Tiers;

        public int MaxMedals => Tiers != null && Tiers.Count > 0 ? Tiers[Tiers.Count - 1].RequiredMedals : 0;
    }
}