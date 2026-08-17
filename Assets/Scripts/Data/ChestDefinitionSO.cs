using System;
using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct ChestDropRate
    {
        public BaseItemDefinitionSO ItemDefinition;
        public int Level;
        public float Weight; 
    }
    
    [Serializable]
    public class ChestData
    {
        [field: SerializeField] public int Level;
        [field: SerializeField] public Sprite ItemIcon;
        [Header("Time & Cost Settings")]
        [field: SerializeField] public int UnlockDurationSeconds;
        [field: SerializeField] public int InstantUnlockGemCost;
        
        [Header("Loot Configuration")] 
        [SerializeField] private List<ChestDropRate> _possibleLoots;
        public IReadOnlyList<ChestDropRate> PossibleLoots => _possibleLoots;
    }
    [CreateAssetMenu(fileName = "NewChestDef", menuName = "Data/Chest Definition")]
    public class ChestDefinitionSO : BaseItemDefinitionSO
    {
        [field: SerializeField] public List<ChestData> Chests { get; private set; }
        
        public override int MaxLevel => Chests != null ? Chests.Count : 0;
        public override Sprite GetIcon(int level)
        {
            var data = GetChestData(level);
            return data?.ItemIcon;
        }
        
        public ChestData GetChestData(int level)
        {
            return Chests.Find(i => i.Level == level);
        }
        
        public override IGridItem CreateRuntimeData(string id, int level)
        {
            return new ChestItemData(id, level);
        }
    }
}