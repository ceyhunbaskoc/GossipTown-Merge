using System;
using System.Threading.Tasks;
using Core.GridSystem;
using Core.SaveSystem;
using Order;
using UnityEngine;

namespace Core.Tutorial
{
    public class TutorialOrchestrator : IDisposable
    {
        private readonly TutorialSaveData _saveData;
        private readonly GridDataModel _gridModel;
        private readonly OrderDataModel _orderDataModel;
        private readonly ITutorialUI _tutorialUI;

        public bool IsCompleted => _saveData.CurrentStep == TutorialStep.Completed;

        public TutorialOrchestrator(
            TutorialSaveData saveData, 
            GridDataModel gridModel, 
            OrderDataModel orderDataModel,
            ITutorialUI tutorialUI)
        {
            _saveData = saveData;
            _gridModel = gridModel;
            _orderDataModel = orderDataModel;
            _tutorialUI = tutorialUI;

            if (!IsCompleted)
            {
                _gridModel.OnItemSpawned += HandleItemMerged;
                _orderDataModel.OnOrderCompleted += HandleOrderCompleted;
            }
        }

        public void StartCurrentStep(Vector3 leftItemPos, Vector3 rightItemPos)
        {
            if (_saveData.CurrentStep == TutorialStep.NotStarted)
            {
                _saveData.CurrentStep = TutorialStep.MergeItems;
            }

            ProcessStep(leftItemPos, rightItemPos);
        }

        private void ProcessStep(Vector3 leftItemPos = default, Vector3 rightItemPos = default)
        {
            switch (_saveData.CurrentStep)
            {
                case TutorialStep.MergeItems:
                    _tutorialUI.HighlightGridCells(leftItemPos, rightItemPos);
                    _tutorialUI.PlayHandAnimation(leftItemPos, rightItemPos);
                    break;
                
                case TutorialStep.CompleteOrder:
                    _tutorialUI.HighlightOrderCompleteButton();
                    break;
                
                case TutorialStep.ShowCoreLoop:
                    _tutorialUI.ShowCoreLoopPanel(FinishTutorial);
                    break;
            }
        }

        private void HandleItemMerged(Vector2Int from, Vector2Int to, ItemData item)
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
            await Task.Delay(2000);
            ProcessStep();
        }

        private void FinishTutorial()
        {
            _saveData.CurrentStep = TutorialStep.Completed;
            
            _gridModel.OnItemSpawned -= HandleItemMerged;
            _orderDataModel.OnOrderCompleted -= HandleOrderCompleted;
        }

        public void Dispose()
        {
            _gridModel.OnItemSpawned -= HandleItemMerged;
            _orderDataModel.OnOrderCompleted -= HandleOrderCompleted;
        }
    }
}