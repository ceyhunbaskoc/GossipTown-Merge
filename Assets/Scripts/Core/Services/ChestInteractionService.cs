using System;
using Core.AdService;
using Core.Controllers;
using Core.Economy;
using Core.Factories;
using Core.GridSystem;
using Data;
using UI.Chest;
using UnityEngine;

namespace Core.Services
{
    public class ChestInteractionService
    {
        private readonly ILootGenerationService _lootService;
        private readonly GridDataModel _gridModel;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly GridItemDataFactory _gridItemDataFactory;
        private readonly MainBoardController _gridController;
        private readonly ChestOpeningOrchestrator _chestOpeningOrchestrator;
        private readonly IEconomyModifier _economyModifier;
        private readonly ITimeManager _timeManager;
        private readonly MergeItemFactory _mergeItemFactory;
        private readonly IAdService _adService;
        
        public ChestInteractionService(
            ILootGenerationService lootService,
            GridDataModel gridModel,
            ItemDatabaseSO itemDatabase,
            GridItemDataFactory gridItemDataFactory,
            MainBoardController gridController,
            ChestOpeningOrchestrator chestOpeningOrchestrator,
            IEconomyModifier economyModifier,
            ITimeManager timeManager,
            MergeItemFactory mergeItemFactory,
            IAdService adService
            )
        {
            _lootService = lootService;
            _gridModel = gridModel;
            _itemDatabase = itemDatabase;
            _gridItemDataFactory = gridItemDataFactory;
            _gridController = gridController;
            _chestOpeningOrchestrator = chestOpeningOrchestrator;
            _economyModifier = economyModifier;
            _timeManager = timeManager;
            _mergeItemFactory = mergeItemFactory;
            _adService = adService;
        }


        public void StartUnlocking(ChestItemData chestItemData)
        {
            if (chestItemData == null || chestItemData.CurrentState != ChestState.Locked) return;
            
            ChestData chestData = GetChestData(chestItemData);
            if (chestData == null) return;

            long targetTicks = _timeManager.CurrentTimeTicks + TimeSpan.FromSeconds(chestData.UnlockDurationSeconds).Ticks;
            chestItemData.StartUnlocking(targetTicks);
            _timeManager.RegisterTimer(chestItemData);
        }

        public bool ForceOpenWithGems(ChestItemData chestItemData)
        {
            if (chestItemData == null || chestItemData.CurrentState != ChestState.Unlocking) return false;
    
            ChestData chestData = GetChestData(chestItemData);
            if (chestData == null) return false;

            if (_economyModifier.TrySpendGems(chestData.InstantUnlockGemCost))
            {
                chestItemData.ForceComplete();
                return true;
            }
    
            return false;
        }

        public void ForceOpenWithAd(ChestItemData chestItemData)
        {
            if (chestItemData == null || chestItemData.CurrentState != ChestState.Unlocking) return;
            
            _adService.ShowRewardedAd(
                onAdWatched: () =>
                {
                    chestItemData.ForceComplete();
                },
                onAdFailed: () =>
                {
                    //nothing
                });
        }

        public void OpenChest(ChestItemData chestItemData, Vector2Int gridPosition)
        {
            if (chestItemData == null || chestItemData.CurrentState != ChestState.ReadyToOpen) return;
            
            ChestData chestData = GetChestData(chestItemData);
            if (chestData == null) return;

            ItemIdentifier chestRewardItem = _lootService.GenerateLootForChest(chestData);
            if (string.IsNullOrEmpty(chestRewardItem.Id))
            {
                Debug.LogWarning($"[ChestInteraction] Sandık (ID: {chestItemData.Id}) için loot üretilemedi!");
                return;
            }

            IViewItem chestVisual = _gridController.ExtractVisualAt(gridPosition);
            _chestOpeningOrchestrator.PlayOpeningSequence(
                chestView: chestVisual, 
                onLidOpened: () => 
                {
                    _gridModel.TryClearCell(gridPosition);
                    IGridItem newLootItem = _gridItemDataFactory.CreateItemData(chestRewardItem.Id, chestRewardItem.Level);
                    _gridModel.TryPlaceObject(gridPosition, newLootItem);
                },
                onSequenceComplete: () => 
                {
                    _mergeItemFactory.RecycleItem(chestVisual);
                }
            );
        }

        private ChestData GetChestData(ChestItemData chestItemData)
        {
            BaseItemDefinitionSO def = _itemDatabase.GetItemDef(chestItemData.Id);
            return (def as ChestDefinitionSO)?.GetChestData(chestItemData.Level);
        }
    }
}