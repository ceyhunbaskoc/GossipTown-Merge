using System;
using Core.GridSystem;
using Core.Economy;
using Core.Services;
using Data;
using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chest
{
    public class ChestRewardHandler : MonoBehaviour
    {
        [Header("Module Container")]
        [SerializeField] private GameObject _chestActionZone;

        [Header("State: Locked")]
        [SerializeField] private GameObject _lockedStateContainer;
        [SerializeField] private Button _startUnlockButton;
        
        [Header("State: Unlocking")]
        [SerializeField] private GameObject _unlockingStateContainer;
        [SerializeField] private Button _speedUpAdButton;
        [SerializeField] private Button _forceOpenGemButton;
        [SerializeField] private TextMeshProUGUI _unlockingTimerText;
        [SerializeField] private TextMeshProUGUI _forceOpenGemValueText;

        [Header("State: Ready")]
        [SerializeField] private GameObject _readyStateContainer;
        [SerializeField] private Button _openChestButton;

        private BoardSelectionService _selectionService;
        private ChestInteractionService _chestInteractionService;
        
        private ChestItemData _activeChestData;
        private ChestData _activeChestStaticData;
        private IWarningMessageService _warningService;

        public void Initialize(BoardSelectionService selectionService, ChestInteractionService chestInteractionService, IWarningMessageService warningService)
        {
            _selectionService = selectionService;
            _chestInteractionService = chestInteractionService;
            _warningService = warningService;

            _selectionService.OnItemSelected += OnItemSelected;
            _selectionService.OnSelectionCleared += HideModule;
            
            _startUnlockButton.onClick.AddListener(OnStartUnlockClicked);
            _speedUpAdButton.onClick.AddListener(OnSpeedUpAdClicked);
            _forceOpenGemButton.onClick.AddListener(OnForceOpenGemClicked);
            _openChestButton.onClick.AddListener(OnOpenChestClicked);
            
            HideModule();
        }

        private void OnItemSelected(IGridItem item, Data.BaseItemDefinitionSO itemDef)
        {
            if (item is ChestItemData chestItem && itemDef is ChestDefinitionSO chestDef)
            {
                UnsubscribeFromActiveChest();

                _activeChestData = chestItem;
                _activeChestStaticData = chestDef.GetChestData(chestItem.Level); 
                
                _activeChestData.OnStateChanged += HandleChestStateChanged;

                _chestActionZone.SetActive(true);
                RefreshUIState();
            }
            else
            {
                HideModule();
            }
        }

        private void HandleChestStateChanged(ChestState newState)
        {
            RefreshUIState();
        }

        private void RefreshUIState()
        {
            if (_activeChestData == null) return;
            
            _forceOpenGemValueText.text = _activeChestStaticData.InstantUnlockGemCost.ToString();

            _lockedStateContainer.SetActive(false);
            _unlockingStateContainer.SetActive(false);
            _readyStateContainer.SetActive(false);

            switch (_activeChestData.CurrentState)
            {
                case ChestState.Locked:
                    _lockedStateContainer.SetActive(true);
                    break;
                case ChestState.Unlocking:
                    _unlockingStateContainer.SetActive(true);
                    break;
                case ChestState.ReadyToOpen:
                    _readyStateContainer.SetActive(true);
                    break;
            }
        }

        private void OnStartUnlockClicked()
        {
            _chestInteractionService.StartUnlocking(_activeChestData);
        }

        private void OnForceOpenGemClicked()
        {
            bool success = _chestInteractionService.ForceOpenWithGems(_activeChestData);
    
            if (!success)
            {
                _warningService.ShowWarning("Not Enough Gems!", Input.mousePosition);
            }
        }

        private void OnSpeedUpAdClicked()
        {
            _chestInteractionService.ForceOpenWithAd(_activeChestData);
        }

        private void OnOpenChestClicked()
        {
            _chestInteractionService.OpenChest(_activeChestData, _selectionService.CurrentSelectedPosition.Value);
        }

        private void HideModule()
        {
            UnsubscribeFromActiveChest();

            _activeChestData = null;
            _activeChestStaticData = null;
            _chestActionZone.SetActive(false);
        }

        private void UnsubscribeFromActiveChest()
        {
            if (_activeChestData != null)
            {
                _activeChestData.OnStateChanged -= HandleChestStateChanged;
            }
        }

        private void Update()
        {
            if (!_chestActionZone.activeInHierarchy || _activeChestData == null) return;

            if (_activeChestData.CurrentState == ChestState.Unlocking)
            {
                long remainingTicks = _activeChestData.UnlockTargetTimeTicks - DateTime.UtcNow.Ticks;
                
                if (remainingTicks > 0)
                {
                    TimeSpan ts = TimeSpan.FromTicks(remainingTicks);
                    _unlockingTimerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
                }
            }
        }

        private void OnDestroy()
        {
            if (_selectionService != null)
            {
                _selectionService.OnItemSelected -= OnItemSelected;
                _selectionService.OnSelectionCleared -= HideModule;
            }
            UnsubscribeFromActiveChest();
            
            _startUnlockButton.onClick.RemoveAllListeners();
            _speedUpAdButton.onClick.RemoveAllListeners();
            _forceOpenGemButton.onClick.RemoveAllListeners();
            _openChestButton.onClick.RemoveAllListeners();
        }
    }
}