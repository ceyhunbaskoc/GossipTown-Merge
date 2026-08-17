using System;
using Core.Services;
using Data.Roadmap;
using UnityEngine;

namespace UI.Map
{
    public class BuildingLevelCheckPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private BuildingNotEnoughLevelView _popupView;
        
        private RoadmapProgressionService _progressionService;
        private MapNodeDefinitionSO _currentNodeDefinition;
        
        private IInputLockService _inputLockService;
        private bool _isPanelCurrentlyOpen;

        public void Initialize(RoadmapProgressionService progressionService, IInputLockService inputLockService)
        {
            _progressionService = progressionService;
            _inputLockService = inputLockService;
            _popupView.OnCloseClicked += ClosePopup;
        }
        
        public void OpenNotEnoughLevelPopup(MapNodeDefinitionSO nodeDefinition)
        {
            if (!_progressionService.IsEnoughLevel(nodeDefinition.NodeId))
            {
                _currentNodeDefinition = nodeDefinition;
                
                if (!_isPanelCurrentlyOpen)
                {
                    _inputLockService.AddLock();
                    _isPanelCurrentlyOpen = true;
                }
                
                UpdatePopupContent();
                _popupView.Open();
            }
        }

        private void UpdatePopupContent()
        {
            string buildingName = _currentNodeDefinition.Building.BuildingName;
            int buildingMaxLevel = _currentNodeDefinition.Building.MaxLevel;
            int requiredLevel = _progressionService.GetNodeRequiredLevel(_currentNodeDefinition.NodeId);
            Sprite icon = _currentNodeDefinition.Building.GetLevelData(buildingMaxLevel).LevelSprite;
            _popupView.SetContent(requiredLevel, buildingName, icon);
        }

        public void ClosePopup()
        {
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
                _isPanelCurrentlyOpen = false;
            }
            _popupView.Close();
        }

        private void OnDestroy()
        {
            if (_isPanelCurrentlyOpen && _inputLockService != null)
            {
                _inputLockService.RemoveLock();
            }
            _popupView.OnCloseClicked -= ClosePopup;
        }
    }
}