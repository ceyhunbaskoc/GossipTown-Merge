using System;
using Core.Economy.Offers;
using Core.GridSystem;
using Core.Services;
using Data;
using TMPro;
using UI.Components;
using UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Spawner
{
    public class SpawnerRestingHandler : MonoBehaviour
    {
        [Header("Module Container")] 
        [SerializeField] private GameObject _spawnerActionZone;

        [Header("State: Resting")] 
        [SerializeField] private GameObject _restingStateContainer;
        [SerializeField] private TextMeshProUGUI _restingTimerText;
        [SerializeField] private Button _speedUpAdButton;
        [SerializeField] private Button _forceOpenGemButton;
        [SerializeField] private TextMeshProUGUI _forceOpenGemValueText;

        private BoardSelectionService _selectionService;
        private SpawnerRestingService _spawnerRestingService;
        private IWarningMessageService _warningService;
        private IFirstTimeOfferService _firstTimeOfferService;
        private FreeSpawnerRefillPanelPresenter _freePanelPresenter;

        private SpawnerItemData _activeSpawnerData;
        private SpawnerData _activeSpawnerStaticData;
        private SpawnerDefinitionSO _spawnerDefinition;

        public void Initialize(BoardSelectionService selectionService,
            SpawnerRestingService spawnerRestingService,
            IWarningMessageService warningService,
            IFirstTimeOfferService firstTimeOfferService,
            FreeSpawnerRefillPanelPresenter freePanelPresenter
            )
        {
            _selectionService = selectionService;
            _spawnerRestingService = spawnerRestingService;
            _warningService = warningService;
            _firstTimeOfferService = firstTimeOfferService;
            _freePanelPresenter = freePanelPresenter;

            _selectionService.OnItemSelected += OnItemSelected;
            _selectionService.OnSelectionCleared += HideModule;
            
            _speedUpAdButton.onClick.AddListener(OnSpeedUpAdClicked);
            _forceOpenGemButton.onClick.AddListener(OnForceOpenGemClicked);

            HideModule();
        }

        private void OnItemSelected(IGridItem item, BaseItemDefinitionSO itemDef)
        {
            if (item is SpawnerItemData spawnerItem && itemDef is SpawnerDefinitionSO spawnerDef)
            {
                UnsubscribeFromActiveSpawner();
                _spawnerDefinition = spawnerDef;

                _activeSpawnerData = spawnerItem;
                _activeSpawnerStaticData = spawnerDef.GetSpawnerData(spawnerItem.Level);

                _activeSpawnerData.OnCooldownStateChanged += HandleCooldownStateChanged;
                
                if (_activeSpawnerData.IsInCooldown && _firstTimeOfferService.IsFreeSpawnerOfferAvailable())
                {
                    _freePanelPresenter.OpenPanel(OnFreeSpawnerClaimed);
                }

                _spawnerActionZone.SetActive(true);
                RefreshUIState();
            }
            else
            {
                HideModule();
            }
        }
        
        private void OnFreeSpawnerClaimed()
        {
            _spawnerRestingService.ForceSkip(_activeSpawnerData);

            _firstTimeOfferService.MarkFreeSpawnerOfferClaimed();
            _freePanelPresenter.ClosePanel();
        }
        
        private void OnForceOpenGemClicked()
        {
            bool success = _spawnerRestingService.ForceSkipWithGems(_activeSpawnerData);
    
            if (!success)
            {
                _warningService.ShowWarning("Not Enough Gems!", Input.mousePosition);
            }
        }

        private void OnSpeedUpAdClicked()
        {
            _spawnerRestingService.ForceSkipWithAd(_activeSpawnerData);
        }

        private void HandleCooldownStateChanged(bool isInCooldown)
        {
            RefreshUIState();
        }

        private void RefreshUIState()
        {
            if (_activeSpawnerData == null) return;
            _restingStateContainer.SetActive(_activeSpawnerData.IsInCooldown);
            if (_activeSpawnerData.IsInCooldown)
            {
                _forceOpenGemValueText.text = _spawnerDefinition.GetRestingSkipCost(_activeSpawnerData.Level).ToString();
            }
        }

        private void HideModule()
        {
            UnsubscribeFromActiveSpawner();

            _activeSpawnerData = null;
            _activeSpawnerStaticData = null;
            _spawnerDefinition = null;
            
            _restingStateContainer.SetActive(false);
            _spawnerActionZone.SetActive(false);
        }

        private void UnsubscribeFromActiveSpawner()
        {
            if (_activeSpawnerData != null)
            {
                _activeSpawnerData.OnCooldownStateChanged -= HandleCooldownStateChanged;
            }
        }

        private void Update()
        {
            if (!_spawnerActionZone.activeInHierarchy || _activeSpawnerData == null) return;

            if (_activeSpawnerData.IsInCooldown)
            {
                long remainingTicks = _activeSpawnerData.TargetTimeTicks - DateTime.UtcNow.Ticks;

                if (remainingTicks > 0)
                {
                    TimeSpan ts = TimeSpan.FromTicks(remainingTicks);
                    _restingTimerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
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
            UnsubscribeFromActiveSpawner();
        }
    }
}