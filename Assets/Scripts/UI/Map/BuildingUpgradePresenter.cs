using UnityEngine;
using Core.Economy;
using System;
using System.Collections.Generic;
using Core.PoolSystem;
using Core.Services;
using Data.Reward;
using Data.Roadmap;
using UI.Level;

namespace UI.Map
{
    public class BuildingUpgradePresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private BuildingUpgradePopupView _popupView;

        private RoadmapProgressionService _progressionService;
        private PlayerEconomyModel _economyModifier;
        
        private MapNodeDefinitionSO _currentNodeDef;
        private IInputLockService _inputLockService;
        private IObjectPool _objectPool;
        private GlobalRewardIconDatabaseSO _iconDatabase;
        
        private bool _isPanelCurrentlyOpen;

        public void Initialize(
            RoadmapProgressionService progressionService,
            PlayerEconomyModel economyModifier,
            IInputLockService inputLockService,
            IObjectPool objectPool,
            GlobalRewardIconDatabaseSO iconDatabase)
        {
            _progressionService = progressionService;
            _economyModifier = economyModifier;
            _inputLockService = inputLockService;
            _objectPool = objectPool;
            _iconDatabase = iconDatabase;

            _popupView.OnUpgradeClicked += HandleUpgradeRequest;
            _popupView.OnCloseClicked += ClosePopup;
            
            _popupView.Close();
        }
        
        private List<LevelRewardItemView> _spawnedRewardViews = new List<LevelRewardItemView>();

        public void OpenPopupForNode(MapNodeDefinitionSO nodeDef)
        {
            if(!_progressionService.IsNodeUnlocked(nodeDef.NodeId))
            {
                Debug.Log($"[Presenter] Node {nodeDef.NodeId} is locked. Popup opening blocked.");
                // TODO: In the future, trigger a UI Toast/Tooltip here (e.g., "Requires building Lvl 2").
                return;
            }
            _currentNodeDef = nodeDef;
            UpdatePopupContent();
            _popupView.Open();
            if (!_isPanelCurrentlyOpen)
            {
                _inputLockService.AddLock();
                _isPanelCurrentlyOpen = true;
            }
        }

        private void UpdatePopupContent()
{
    if (_spawnedRewardViews.Count > 0)
    {
        foreach (var view in _spawnedRewardViews)
        {
            _objectPool.Despawn(PoolObjectType.LevelRewardItemViewNoEffect, view);
        }
        _spawnedRewardViews.Clear();
    }
    
    if (_currentNodeDef == null || _currentNodeDef.Building == null) return;

    string name = _currentNodeDef.Building.BuildingName;
    int currentLevel = _progressionService.GetNodeCurrentLevel(_currentNodeDef.NodeId);
    int maxLevel = _currentNodeDef.Building.MaxLevel;
    
    string progress = $"{currentLevel} / {maxLevel}";
    bool isMaxLevel = currentLevel >= maxLevel;

    Sprite icon = null;
    int cost = 0;
    bool canAfford = false;
    string goldProgress = string.Empty;

    BuildingLevelData currentLevelData = _currentNodeDef.Building.GetLevelData(currentLevel);

    if (isMaxLevel)
    {
        icon = currentLevelData?.LevelSprite;
        goldProgress = "MAX";
        canAfford = false;
    }
    else
    {
        BuildingLevelData nextLevelData = _currentNodeDef.Building.GetLevelData(currentLevel + 1);
        
        if (nextLevelData != null)
        {
            icon = nextLevelData.LevelSprite;
            cost = nextLevelData.UpgradeCost;
            canAfford = _economyModifier.HasEnoughGold(cost); 
            
            goldProgress = canAfford ? $"{cost} / {cost}" : $"{_economyModifier.Golds} / {cost}";

            if (nextLevelData.LevelRewards != null)
            {
                foreach (var reward in nextLevelData.LevelRewards)
                {
                    if (reward == null) continue;

                    bool isCurrency = reward.Category == Data.Quests.RewardCategory.Gold || 
                                      reward.Category == Data.Quests.RewardCategory.Gem || 
                                      reward.Category == Data.Quests.RewardCategory.Energy;

                    if (!isCurrency && reward.RewardItem == null)
                    {
                        continue;
                    }

                    Sprite rewardIcon = isCurrency 
                        ? _iconDatabase.GetIconForCategory(reward.Category) 
                        : reward.RewardItem.GetIcon(reward.Level);

                    string formattedText = FormatRewardText(reward);

                    LevelRewardItemView rewardItemView = _objectPool.SpawnUI<LevelRewardItemView>(PoolObjectType.LevelRewardItemViewNoEffect, _popupView.RewardsContainer);
                    rewardItemView.Setup(rewardIcon, formattedText);
                    _spawnedRewardViews.Add(rewardItemView);
                }
            }
        }
        else
        {
            Debug.LogError($"[BuildingUpgradePresenter] Veri Hatası: {name} için {currentLevel + 1}. seviye verisi bulunamadı!");
        }
    }

    _popupView.SetContent(name, progress, goldProgress, icon, canAfford, isMaxLevel);
}
        
        private string FormatRewardText(BuildingRewardDefinition reward)
        {
            switch (reward.Category)
            {
                case Data.Quests.RewardCategory.ItemSpawner:
                case Data.Quests.RewardCategory.Chest:
                    string itemName = reward.RewardItem != null ? reward.RewardItem.ItemName : "Unknown";
                    return $"{itemName} x{reward.Amount}";

                case Data.Quests.RewardCategory.Gem:
                case Data.Quests.RewardCategory.Gold:
                case Data.Quests.RewardCategory.Energy:
                    return $"x{reward.Amount}";

                default:
                    return $"x{reward.Amount}";
            }
        }

        private void HandleUpgradeRequest()
        {
            if (_currentNodeDef == null) return;
            _progressionService.TryUpgradeNode(_currentNodeDef.NodeId);

            ClosePopup();
        }

        private void ClosePopup()
        {
            _currentNodeDef = null;
            _popupView.Close();
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
                _isPanelCurrentlyOpen = false;
            }
        }

        private void OnDestroy()
        {
            if (_isPanelCurrentlyOpen && _inputLockService != null)
            {
                _inputLockService.RemoveLock();
            }
            if (_popupView != null)
            {
                _popupView.OnUpgradeClicked -= HandleUpgradeRequest;
                _popupView.OnCloseClicked -= ClosePopup;
            }
        }
    }
}