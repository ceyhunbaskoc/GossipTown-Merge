using System;
using System.Collections.Generic;
using Core.Economy;
using UnityEngine;
using Core.Services;
using Data.Roadmap;
using Core.PoolSystem;

namespace UI.Map
{
    public class RoadmapUIController : MonoBehaviour
    {
        [Header("Map Elements")]
        [SerializeField] private List<BuildingSlotView> _mapSlots;

        private RoadmapProgressionService _progressionService;
        private PlayerEconomyModel _economyModel;
        private IInputLockService _inputLockService;
        
        public event Action<string> OnNodeClickedRequested;
        public event Action<string> OnNodeUpgradeVisualCompleted; 

        public void Initialize(
            RoadmapProgressionService progressionService, 
            PlayerEconomyModel economyModel,
            IInputLockService inputLockService,
            IObjectPool objectPool,
            Sprite unBuildSprite)
        {
            _progressionService = progressionService;
            _economyModel = economyModel;
            _inputLockService = inputLockService;

            _progressionService.OnNodeUpgraded += _handleNodeUpgraded;
            _progressionService.OnNodeUnlocked += _handleNodeUnlocked;
            _progressionService.OnDataLoaded += _refreshAllSlots;
            _economyModel.OnGoldChanged += _handleGoldChanged;

            foreach (var slot in _mapSlots)
            {
                slot.Initialize(objectPool, unBuildSprite);
                
                slot.OnSlotClicked += _handleSlotClicked;
                slot.OnUpgradeAnimationCompleted += _handleSlotAnimationCompleted;
                
                _refreshSlotVisual(slot);
            }
        }

        private void _handleNodeUpgraded(MapNodeDefinitionSO nodeDef, BuildingLevelData nextLevelData)
        {
            _inputLockService.AddUILock();
            _inputLockService.AddLock();

            foreach (var slot in _mapSlots)
            {
                string nodeId = slot.NodeDefinition.NodeId;
        
                int currentLevel = _progressionService.GetNodeCurrentLevel(nodeId);
                bool canUpgrade = _progressionService.CanUpgradeNode(nodeId);
                bool isUnlocked = _progressionService.IsNodeUnlocked(nodeId);

                bool shouldAnimate = (nodeId == nodeDef.NodeId);
        
                slot.UpdateVisualState(currentLevel, canUpgrade, isUnlocked, shouldAnimate);
            }
        }

        private void _handleSlotAnimationCompleted(string nodeId)
        {
            _inputLockService.RemoveUILock();
            _inputLockService.RemoveLock();
            
            OnNodeUpgradeVisualCompleted?.Invoke(nodeId);
        }

        private void _handleSlotClicked(string nodeId)
        {
            if (_inputLockService.IsLocked || _inputLockService.IsUILocked) return;
            
            OnNodeClickedRequested?.Invoke(nodeId);
        }

        private void _handleNodeUnlocked(MapNodeDefinitionSO nodeDef)
        {
            _refreshAllSlots();
        }
        
        private void _handleGoldChanged(int currentGold)
        {
            _refreshAllSlots();
        }

        private void _refreshAllSlots()
        {
            foreach (var slot in _mapSlots)
            {
                _refreshSlotVisual(slot);
            }
        }

        private void _refreshSlotVisual(BuildingSlotView slot)
        {
            string nodeId = slot.NodeDefinition.NodeId;
            int currentLevel = _progressionService.GetNodeCurrentLevel(nodeId);
            bool canUpgrade = _progressionService.CanUpgradeNode(nodeId);
            bool isUnlocked = _progressionService.IsNodeUnlocked(nodeId);

            slot.UpdateVisualState(currentLevel, canUpgrade, isUnlocked);
        }

        private void OnDestroy()
        {
            if (_progressionService != null)
            {
                _progressionService.OnNodeUpgraded -= _handleNodeUpgraded;
                _progressionService.OnNodeUnlocked -= _handleNodeUnlocked;
                _progressionService.OnDataLoaded -= _refreshAllSlots;
            }
            if (_economyModel != null)
            {
                _economyModel.OnGoldChanged -= _handleGoldChanged;
            }

            foreach (var slot in _mapSlots)
            {
                slot.OnSlotClicked -= _handleSlotClicked;
                slot.OnUpgradeAnimationCompleted -= _handleSlotAnimationCompleted;
            }
        }
    }
}