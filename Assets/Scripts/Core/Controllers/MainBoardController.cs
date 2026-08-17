using System.Collections.Generic;
using Core.GridSystem;
using Core.Services;
using Data;
using Data.Quests;
using Data.Reward;
using UI.Collectible;
using UI.Components;
using UI.FlightSystem;
using UnityEngine;

namespace Core.Controllers
{
    public class MainBoardController : BaseGridViewController
    {
        private MergeService _mergeService;
        private ItemInteractionService _itemInteractionService;
        private BoardTransferService _transferService;
        private IWarningMessageService _warningService;
        private CollectibleCollectOrchestrator _collectibleCollectOrchestrator;
        private CurrencyFlightService _currencyFlightService;
        private ItemDatabaseSO _itemDatabase;
        private MergeVFXOrchestrator _mergeVFXOrchestrator;
        
        
        private Camera _mainCamera; 
        
        public BoardSelectionService SelectionService => _selectionService;

        public void InitializeMainBoard(
            GridDataModel gridModel, 
            MergeService mergeService, 
            ItemInteractionService itemInteractionService,
            BoardTransferService transferService,
            BoardSelectionService selectionService,
            IWarningMessageService warningService, 
            CollectibleCollectOrchestrator collectibleCollectOrchestrator,
            CurrencyFlightService currencyFlightService,
            ItemDatabaseSO itemDatabase,
            MergeVFXOrchestrator mergeVFXOrchestrator,
            int width, 
            int height, 
            float cellSize)
        {
            _mergeService = mergeService;
            _itemInteractionService = itemInteractionService;
            _transferService = transferService;
            _warningService = warningService;
            _collectibleCollectOrchestrator = collectibleCollectOrchestrator;
            _currencyFlightService = currencyFlightService;
            _itemDatabase = itemDatabase;
            _mergeVFXOrchestrator = mergeVFXOrchestrator;
            
            _mainCamera = Camera.main; 
            
            base.Initialize(gridModel, width, height, cellSize, selectionService);
        }

        public override void OnItemDropRequested(IViewItem viewItem, Vector3 dropWorldPosition)
        {
            TriggerInteractionEvent();
            Vector2Int newGridPos = WorldToGridPosition(dropWorldPosition);
            Vector2Int oldGridPos = viewItem.CurrentGridPosition;

            if (newGridPos == oldGridPos) 
            {
                viewItem.SnapBackToStart();
                return;
            }

            if (_gridModel.IsCellOccupied(newGridPos))
            {
                if (_mergeService.TryProcessMerge(oldGridPos, newGridPos))
                {
                    _selectionService.SelectItemAt(newGridPos);
                    Vector3 targetWorldPosition = GridToWorldPosition(newGridPos);
                    _mergeVFXOrchestrator.PlayMergeEffect(targetWorldPosition);
                }
                else
                {
                    viewItem.SnapBackToStart();
                }
            }
            else
            {
                if (!_gridModel.TryMoveObject(oldGridPos, newGridPos)) 
                    viewItem.SnapBackToStart();
            }
        }

        public override void OnItemInteractRequested(Vector2Int gridPosition, IGridItem gridItem)
        {
            TriggerInteractionEvent();

            bool isCollectible = gridItem is CollectibleItemData;
            IViewItem collectibleVisual = null;

            if (isCollectible)
            {
                collectibleVisual = ExtractVisualAt(gridPosition); 
            }

            InteractionResult result = _itemInteractionService.ProcessInteraction(gridPosition);

            if (result == InteractionResult.CollectibleCollected && collectibleVisual != null)
            {
                var collectibleDef = _itemDatabase.GetItemDef(gridItem.Id) as CollectibleItemDefinitionSO;
                CurrencyType currency = collectibleDef.RewardCurrency;
                int amount = collectibleDef.GetRewardAmount(gridItem.Level);

                Vector3 startWorldPosition = Vector3.zero;
                if (collectibleVisual is Component visualComp)
                {
                    startWorldPosition = visualComp.transform.position;
                }

                _collectibleCollectOrchestrator.PlayCollectSequence(collectibleVisual, () => 
                {
                    _itemFactory.RecycleItem(collectibleVisual);
            
                    RewardCategory category = CurrencyTypeToRewardCategory(currency);
                    _currencyFlightService.PlayFlightAnimation(category, amount, startWorldPosition);
                });
            }
            else if (result != InteractionResult.Success && isCollectible && collectibleVisual != null)
            {
                _visualRegistry.Add(gridItem, collectibleVisual);
            }
            else if(result != InteractionResult.Success)
            {
                Vector3 worldPos = GridToWorldPosition(gridPosition);
                Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

                switch (result)
                {
                    case InteractionResult.AwesomeSuccess:
                        _warningService.ShowWarning("Awesome!", screenPos, InteractionResult.AwesomeSuccess);
                        break;
                    case InteractionResult.GridFull:
                        _warningService.ShowWarning("Grid is Full!", screenPos);
                        break;
                    case InteractionResult.NoEnergy:
                        _warningService.ShowWarning("Out of Energy!", screenPos);
                        break;
                    case InteractionResult.Cooldown:
                        _warningService.ShowWarning("Machine is resting!", screenPos);
                        break;
                    case InteractionResult.Invalid:
                        break;
                }
            }
        }

        private RewardCategory CurrencyTypeToRewardCategory(CurrencyType currency)
        {
            RewardCategory result = RewardCategory.Energy;
            switch (currency)
            {
                case CurrencyType.Energy:
                    result = RewardCategory.Energy;
                    break;
                case CurrencyType.Gold:
                    result = RewardCategory.Gold;
                    break;
                case CurrencyType.Gem:
                    result = RewardCategory.Gem;
                    break;
            }

            return result;
        }

        public List<IViewItem> ExtractVisualsForOrder(IReadOnlyDictionary<ItemIdentifier, int> requiredItems)
        {
            List<IViewItem> flyingVisuals = new List<IViewItem>();
            List<Vector2Int> cellsToClear = new List<Vector2Int>();
            Dictionary<ItemIdentifier, int> remainingToFind = new Dictionary<ItemIdentifier, int>(requiredItems);

            for (int x = 0; x < _gridModel.Width; x++)
            {
                for (int y = 0; y < _gridModel.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    IGridItem item = _gridModel.GetItemAt(pos);

                    if (item != null && !_gridModel.IsCellLocked(pos))
                    {
                        ItemIdentifier identifier = new ItemIdentifier(item.Id, item.Level);

                        if (remainingToFind.ContainsKey(identifier) && remainingToFind[identifier] > 0)
                        {
                            remainingToFind[identifier]--;
                            cellsToClear.Add(pos);

                            if (_visualRegistry.TryGetValue(item, out IViewItem viewItem))
                            {
                                flyingVisuals.Add(viewItem);
                                if (viewItem is MonoBehaviour viewBehaviour)
                                {
                                    viewBehaviour.transform.SetParent(null);
                                }
                                _visualRegistry.Remove(item); 
                            }
                        }
                    }
                }
            }
            
            foreach (var pos in cellsToClear)
            {
                _gridModel.TryClearCell(pos);
            }

            return flyingVisuals;
        }
    }
}