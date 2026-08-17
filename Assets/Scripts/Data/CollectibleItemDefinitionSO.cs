using System;
using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;
using Core.Interaction;

namespace Data
{
    public enum CurrencyType { None, Energy, Gold, Gem }
    [Serializable]
    public class CollectibleLevelData
    {
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int RewardAmount { get; private set; }
    }

    [CreateAssetMenu(fileName = "NewCollectibleItem", menuName = "MergeGame/Collectible Item SO")]
    public class CollectibleItemDefinitionSO : BaseItemDefinitionSO, ICollectibleDefinition
    {
        [field: Header("Reward Settings")]
        [field: SerializeField] public CurrencyType RewardCurrency { get; private set; }
        
        [field: Header("Level Settings")]
        [field: SerializeField] public List<CollectibleLevelData> LevelDataList { get; private set; }
        
        public override int MaxLevel => LevelDataList != null ? LevelDataList.Count : 0;

        public override Sprite GetIcon(int level)
        {
            var data = LevelDataList.Find(i => i.Level == level);
            return data?.Icon;
        }

        public int GetRewardAmount(int currentLevel)
        {
            var data = LevelDataList.Find(i => i.Level == currentLevel);
            return data != null ? data.RewardAmount : 0;
        }
        
        public override IGridItem CreateRuntimeData(string id, int level)
        {
            return new CollectibleItemData(id, level);
        }
    }
}