using System;
using System.Threading.Tasks;
using Core.GridSystem;
using Core.SaveSystem;
using Order;
using UI.Orders;
using UI.Tutorial;
using UnityEngine;

namespace Core.Tutorial
{
    public class TutorialOrchestrator : IDisposable
    {
        private readonly TutorialSaveData _saveData;
        private readonly GridDataModel _gridModel;
        private readonly OrderDataModel _orderDataModel;
        private readonly ITutorialUI _tutorialUI;
        private readonly OrderUIManager _orderUIManager;
        private RectTransform _cachedCompleteButtonRect;
        private float _cellSize;

        public bool IsCompleted => _saveData.CurrentStep == TutorialStep.Completed;
        
        public TutorialSaveData GetSaveData() => _saveData;

        public TutorialOrchestrator(
            TutorialSaveData saveData, 
            GridDataModel gridModel, 
            OrderDataModel orderDataModel,
            ITutorialUI tutorialUI,
            OrderUIManager orderUIManager,
            float cellSize)
        {
            _saveData = saveData;
            _gridModel = gridModel;
            _orderDataModel = orderDataModel;
            _tutorialUI = tutorialUI;
            _orderUIManager = orderUIManager;
            _cellSize = cellSize;

            if (!IsCompleted)
            {
                _gridModel.OnItemPlaced += HandleItemMerged;
                _orderDataModel.OnOrderCompleted += HandleOrderCompleted;
                _orderUIManager.OnOrderUIGenerated += HandleOrderUIGenerated;
            }
        }
        
        private void HandleOrderUIGenerated(OrderModel model, RectTransform buttonRect)
        {
            _cachedCompleteButtonRect = buttonRect;
        }

        public async void StartCurrentStep(Vector3 leftItemPos, Vector3 rightItemPos)
        {
            await Task.Yield();
        
            if (_saveData.CurrentStep == TutorialStep.NotStarted)
            {
                _saveData.CurrentStep = TutorialStep.MergeItems;
            }
        
            ProcessStep(leftItemPos, rightItemPos);
        }
        
        private async void ProcessStep(Vector3 leftItemPos = default, Vector3 rightItemPos = default)
        {
            switch (_saveData.CurrentStep)
            {
                case TutorialStep.MergeItems:
                    _tutorialUI.HighlightGridCells(leftItemPos, rightItemPos, _cellSize);
                    _tutorialUI.PlayHandAnimation(leftItemPos, rightItemPos);
                    break;
                
                case TutorialStep.CompleteOrder:
                    while (_cachedCompleteButtonRect == null || !_cachedCompleteButtonRect.gameObject.activeInHierarchy)
                    {
                        await Task.Yield();
                    }
                    await Task.Delay(150); 
        
                    _tutorialUI.HighlightOrderCompleteButton(_cachedCompleteButtonRect);
                    break;
                
                case TutorialStep.ShowCoreLoop:
                    _tutorialUI.ShowCoreLoopPanel(FinishTutorial);
                    break;
            }
        }

        private void HandleItemMerged(Vector2Int vector2Int, IGridItem item)
        {
            if (_saveData.CurrentStep == TutorialStep.MergeItems)
            {
                _saveData.CurrentStep = TutorialStep.CompleteOrder;
                ProcessStep();
            }
        }

        private void HandleOrderCompleted(OrderModel completedOrder)
        {
            if (_saveData.CurrentStep == TutorialStep.CompleteOrder)
            {
                _saveData.CurrentStep = TutorialStep.ShowCoreLoop;
                _tutorialUI.ClearHighlights();
                
                ShowCoreLoopWithDelay();
            }
        }

        private async void ShowCoreLoopWithDelay()
        {
            await Task.Delay(1000);
            ProcessStep();
        }

        private void FinishTutorial()
        {
            _saveData.CurrentStep = TutorialStep.Completed;
            
            _gridModel.OnItemPlaced -= HandleItemMerged;
            _orderDataModel.OnOrderCompleted -= HandleOrderCompleted;
        }

        public void Dispose()
        {
            _gridModel.OnItemPlaced -= HandleItemMerged;
            _orderDataModel.OnOrderCompleted -= HandleOrderCompleted;
        }
    }
}