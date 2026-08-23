using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;
using Core.GridSystem;

namespace Data
{
    [Serializable]
    public struct SpawnDropRate
    {
        public int Level;
        public float Weight; 
    }

    [Serializable]
    public class SpawnerData
    {
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public Sprite ItemIcon { get; private set; }
        
        [Header("Spawner Rules")]
        [field: SerializeField] public List<SpawnDropRate> DropRates { get; private set; }
        
        [field: SerializeField] public int MaxCapacity { get; private set; } = 15;
        [field: SerializeField] public float CooldownSeconds { get; private set; } = 300f;
    }

    [CreateAssetMenu(fileName = "NewSpawnerItem", menuName = "MergeGame/Spawner Item SO")]
    public class SpawnerDefinitionSO : BaseItemDefinitionSO, IInteractableDefinition 
    {
        [field: Header("Global Spawner Rules")]
        [field: SerializeField] public BaseItemDefinitionSO SpawnItemSO { get; private set; } 
        [field: SerializeField] public List<SpawnerData> Spawners { get; private set; }
        
        public override int MaxLevel => Spawners != null ? Spawners.Count : 0;

        public override Sprite GetIcon(int level)
        {
            var data = GetSpawnerData(level);
            return data?.ItemIcon;
        }
        
        public SpawnerData GetSpawnerData(int level)
        {
            return Spawners.Find(i => i.Level == level);
        }

        public bool CanInteract(int currentLevel)
        {
            var data = GetSpawnerData(currentLevel);
            return data != null && SpawnItemSO != null && data.DropRates.Count > 0;
        }

        public ItemIdentifier GetDropItem(int currentLevel)
        {
            var data = GetSpawnerData(currentLevel);
            if (!CanInteract(currentLevel))
            {
                throw new Exception($"[SpawnerDefinitionSO] Cannot interact with level {currentLevel}");
            }

            float totalWeight = 0f;
            foreach (var drop in data.DropRates)
            {
                totalWeight += drop.Weight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var drop in data.DropRates)
            {
                currentWeight += drop.Weight;
                if (randomValue <= currentWeight)
                {
                    return new ItemIdentifier(SpawnItemSO.Id, drop.Level);
                }
            }
            return new ItemIdentifier(SpawnItemSO.Id, data.DropRates[0].Level);
        }

        public override IGridItem CreateRuntimeData(string id, int level)
        {
            SpawnerData sData = GetSpawnerData(level);
            int maxCapacity = sData.MaxCapacity;
            return new SpawnerItemData(id, level, maxCapacity);
        }
    }
}