using System;
using Core.Economy;
using Core.Factories;
using Core.GridSystem;
using Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Services
{
    public enum GeneratorResult { Success, AwesomeSuccess, GridFull, OutOfEnergy, Cooldown, Invalid }

    public class GeneratorService : IGeneratorService
    {
        private readonly IGridModifier _gridModifier;
        private readonly IReadOnlyGrid _readOnlyGrid;
        private readonly ItemDatabaseSO _database;
        private readonly PlayerEconomyModel _economyModel;
        private readonly GridItemDataFactory _dataFactory;
        private readonly ITimeManager _timeManager;

        public GeneratorService(
            IGridModifier gridModifier, 
            IReadOnlyGrid readOnlyGrid, 
            ItemDatabaseSO database, 
            PlayerEconomyModel economyModel, 
            GridItemDataFactory dataFactory,
            ITimeManager timeManager)
        {
            _gridModifier = gridModifier;
            _readOnlyGrid = readOnlyGrid;
            _database = database;
            _economyModel = economyModel;
            _dataFactory = dataFactory;
            _timeManager = timeManager;
        }

        public GeneratorResult TryGenerateItem(Vector2Int spawnerPosition, IGridItem spawnerItem)
        {
            if (_readOnlyGrid.IsCellLocked(spawnerPosition)) return GeneratorResult.Invalid;

            SpawnerDefinitionSO spawnerDef = _database.GetSpawnerDef(spawnerItem.Id);
            if (spawnerDef == null) return GeneratorResult.Invalid;

            SpawnerData spawnerData = spawnerDef.GetSpawnerData(spawnerItem.Level);
            if (spawnerData == null || spawnerData.DropRates == null || spawnerData.DropRates.Count == 0) 
                return GeneratorResult.Invalid;

            if (spawnerItem is not SpawnerItemData itemData) return GeneratorResult.Invalid;

            if (itemData.IsInCooldown)
            {
                if (_timeManager.CurrentTimeTicks < itemData.CooldownEndTimeTicks)
                {
                    return GeneratorResult.Cooldown;
                }
                itemData.WakeUp();
            }

            Vector2Int? targetEmptyCell = _readOnlyGrid.FindNearestEmptyCell(spawnerPosition);
            if (!targetEmptyCell.HasValue)
            {
                return GeneratorResult.GridFull;
            }

            if (!_economyModel.TrySpendEnergy(1))
            {
                return GeneratorResult.OutOfEnergy;
            }

            int generatedLevel = GetRandomLevelByWeight(spawnerData);
            IGridItem newItemData = _dataFactory.CreateItemData(spawnerData.SpawnItemSO.Id, generatedLevel);
            
            bool isSpawned = _gridModifier.TrySpawnObject(spawnerPosition, targetEmptyCell.Value, newItemData);
            
            if (isSpawned)
            {
                itemData.CurrentCapacity--;
                
                if (itemData.CurrentCapacity <= 0)
                {
                    long cooldownTicks = (long)(spawnerData.CooldownSeconds * TimeSpan.TicksPerSecond);
                    long targetTimeTicks = _timeManager.CurrentTimeTicks + cooldownTicks;
    
                    itemData.EnterCooldown(targetTimeTicks);
                    _timeManager.RegisterTimer(itemData);
                }
                int realTargetLevel = GetTargetLevel(spawnerData);
                if(generatedLevel > realTargetLevel)
                {
                    return GeneratorResult.AwesomeSuccess;
                }
                return GeneratorResult.Success;
            }

            return GeneratorResult.Invalid;
        }

        private int GetRandomLevelByWeight(SpawnerData spawnerData)
        {
            float totalWeight = 0;
            foreach (var rate in spawnerData.DropRates)
            {
                totalWeight += rate.Weight;
            }
            
            float randomVal = Random.Range(0, totalWeight);
            float cursor = 0;

            foreach (var rate in spawnerData.DropRates)
            {
                cursor += rate.Weight;
                if (randomVal <= cursor)
                {
                    return rate.Level;
                }
            }
            return spawnerData.DropRates[0].Level; 
        }

        private int GetTargetLevel(SpawnerData spawnerData)
        {
            int targetLevel = 1;
            float maxWeight = 0;
            foreach (var rate in spawnerData.DropRates)
            {
                if (rate.Weight > maxWeight)
                {
                    targetLevel = rate.Level;
                    maxWeight = rate.Weight;
                }
            }

            return targetLevel;
        }
    }
}