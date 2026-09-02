using Core.Audio;
using UnityEngine;
using Core.GridSystem;
using Core.Economy;
using Core.Haptics;
using Data;
using Core.Interaction;
using Data.Audio;

namespace Core.Services
{
    public enum InteractionResult { None, Success, AwesomeSuccess, GridFull, NoEnergy, Cooldown, CollectibleCollected, Invalid }

    public class ItemInteractionService
    {
        private readonly GridDataModel _gridModel;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly IEconomyModifier _economyModifier;
        private readonly IGeneratorService _generatorService;
        private readonly ChestInteractionService _chestInteractionService;
        private readonly IAudioService _audioService;
        private readonly IHapticService _hapticService;

        public ItemInteractionService(
            GridDataModel gridModel, 
            ItemDatabaseSO itemDatabase, 
            IEconomyModifier economyModifier,
            IGeneratorService generatorService,
            ChestInteractionService chestInteractionService,
            IAudioService audioService,
            IHapticService hapticService)
        {
            _gridModel = gridModel;
            _itemDatabase = itemDatabase;
            _economyModifier = economyModifier;
            _generatorService = generatorService;
            _chestInteractionService = chestInteractionService;
            _audioService = audioService;
            _hapticService = hapticService;
        }

        public InteractionResult ProcessInteraction(Vector2Int gridPosition)
        {
            IGridItem item = _gridModel.GetItemAt(gridPosition);
            if (item == null) return InteractionResult.Invalid;

            switch (item)
            {
                case ChestItemData chestItem:
                    if (chestItem.CurrentState == ChestState.ReadyToOpen)
                    {
                        _chestInteractionService.OpenChest(chestItem, gridPosition);
                        return InteractionResult.Success;
                    }
                    return InteractionResult.Invalid;

                case SpawnerItemData spawnerItem:
                    GeneratorResult genResult = _generatorService.TryGenerateItem(gridPosition, spawnerItem);
                    if (genResult == GeneratorResult.Success || genResult == GeneratorResult.AwesomeSuccess)
                    {
                        _audioService.PlaySFX(SfxId.Gameplay_ItemSpawn);
                        _hapticService.Play(HapticType.Selection);
                    }
                    return MapGeneratorResult(genResult);

                case CollectibleItemData collectibleItem:
                    return ProcessCollectibleInteraction(collectibleItem, gridPosition);

                default:
                    return InteractionResult.Invalid;
            }
        }

        private InteractionResult ProcessCollectibleInteraction(CollectibleItemData collectibleItem, Vector2Int gridPosition)
        {
            BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(collectibleItem.Id);
        
            if (itemDef is CollectibleItemDefinitionSO collectibleDef)
            {
                int rewardAmount = collectibleDef.GetRewardAmount(collectibleItem.Level);
        
                ApplyReward(collectibleDef.RewardCurrency, rewardAmount);
                _gridModel.TryClearCell(gridPosition);
        
                return InteractionResult.CollectibleCollected; 
            }
    
            return InteractionResult.Invalid;
        }

        private void ApplyReward(CurrencyType currency, int amount)
        {
            switch (currency)
            {
                case CurrencyType.Energy:
                    _economyModifier.AddEnergy(amount);
                    break;
                case CurrencyType.Gold:
                    _economyModifier.AddGold(amount);
                    break;
                case CurrencyType.Gem:
                    _economyModifier.AddGem(amount);
                    break;
                default:
                    Debug.LogWarning($"[Economy] Invalid currency type: {currency}");
                    break;
            }
        }

        private InteractionResult MapGeneratorResult(GeneratorResult genResult)
        {
            return genResult switch
            {
                GeneratorResult.Success => InteractionResult.Success,
                GeneratorResult.GridFull => InteractionResult.GridFull,
                GeneratorResult.OutOfEnergy => InteractionResult.NoEnergy,
                GeneratorResult.Cooldown => InteractionResult.Cooldown,
                GeneratorResult.AwesomeSuccess => InteractionResult.AwesomeSuccess,
                _ => InteractionResult.Invalid
            };
        }
    }
}