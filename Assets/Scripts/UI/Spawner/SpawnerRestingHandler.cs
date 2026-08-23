using System;
using Core.GridSystem;
using Core.Services;
using Data;
using TMPro;
using UnityEngine;

namespace UI.Spawner
{
    public class SpawnerRestingHandler : MonoBehaviour
    {
        [Header("Module Container")] 
        [SerializeField] private GameObject _spawnerActionZone;

        [Header("State: Resting")] 
        [SerializeField] private GameObject _restingStateContainer;
        [SerializeField] private TextMeshProUGUI _restingTimerText;

        private BoardSelectionService _selectionService;

        private SpawnerItemData _activeSpawnerData;
        private SpawnerData _activeSpawnerStaticData;

        public void Initialize(BoardSelectionService selectionService)
        {
            _selectionService = selectionService;

            _selectionService.OnItemSelected += OnItemSelected;
            _selectionService.OnSelectionCleared += HideModule;

            HideModule();
        }

        private void OnItemSelected(IGridItem item, BaseItemDefinitionSO itemDef)
        {
            if (item is SpawnerItemData spawnerItem && itemDef is SpawnerDefinitionSO spawnerDef)
            {
                UnsubscribeFromActiveSpawner();

                _activeSpawnerData = spawnerItem;
                _activeSpawnerStaticData = spawnerDef.GetSpawnerData(spawnerItem.Level);

                _activeSpawnerData.OnCooldownStateChanged += HandleCooldownStateChanged;

                _spawnerActionZone.SetActive(true);
                RefreshUIState();
            }
            else
            {
                HideModule();
            }
        }

        private void HandleCooldownStateChanged(bool isInCooldown)
        {
            RefreshUIState();
        }

        private void RefreshUIState()
        {
            if (_activeSpawnerData == null) return;

            _restingStateContainer.SetActive(_activeSpawnerData.IsInCooldown);
        }

        private void HideModule()
        {
            UnsubscribeFromActiveSpawner();

            _activeSpawnerData = null;
            _activeSpawnerStaticData = null;
            
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