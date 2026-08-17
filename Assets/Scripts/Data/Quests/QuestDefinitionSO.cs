using System;
using System.Collections.Generic;
using Core.Reward;
using Data.Reward;
using UnityEngine;

namespace Data.Quests
{
    public enum QuestType 
    { 
        MergeItem, 
        UpgradeBuilding, 
        SpendGold, 
        CompleteOrder 
    }

    public enum RewardCategory
    {
        ItemSpawner,
        Chest,
        Gold,
        Gem,
        Energy,
        Experience
    }

    [Serializable]
    public class QuestRewardConfig
    {
        public RewardCategory Category;
        public BaseItemDefinitionSO ItemDefinition;
        public int Level = 1;
        public int Amount = 1;

        public RewardPayload ToPayload()
        {
            return new RewardPayload(ItemDefinition.Id, Level, Amount);
        }
        public Sprite GetRewardIcon(GlobalRewardIconDatabaseSO globalDatabase)
        {
            if (ItemDefinition != null)
            {
                return ItemDefinition.GetIcon(Level);
            }
            
            if (globalDatabase != null)
            {
                return globalDatabase.GetIconForCategory(Category);
            }

            return null;
        }
    }

    [CreateAssetMenu(fileName = "NewQuestDef", menuName = "GameData/Quests/QuestDefinition")]
    public class QuestDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string QuestId { get; private set; }
        [field: SerializeField] public QuestType Type { get; private set; }
        [field: SerializeField] public int TargetAmount { get; private set; }
        [field: SerializeField] public int MedalReward { get; private set; }
        [SerializeField] private BaseItemDefinitionSO _requiredItemDef;
        public string RequiredItemId => _requiredItemDef != null ? _requiredItemDef.Id : string.Empty;
        
        [field: SerializeField] public List<QuestRewardConfig> Rewards { get; private set; }
    }
   
}