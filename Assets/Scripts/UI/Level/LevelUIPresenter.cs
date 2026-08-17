using System.Collections.Generic;
using Core.LevelSystem;
using Core.PoolSystem;
using Core.Services;
using Data.Level;
using Data.Quests;
using Data.Reward;
using Data.Roadmap;
using UI.FlightSystem;
using UnityEngine;

namespace UI.Level
{
    public class LevelUIPresenter : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private LevelHUDView _hudView;
        [SerializeField] private LevelWindowView _windowView;
        [SerializeField] private LevelUpWindowView _levelUpWindowView;
        
        private IReadOnlyLevel _levelModel;
        private LevelRewardSettingsSO _rewardSettings;
        private LevelProgressionSettingsSO _progressionSettings;
        private GlobalRewardIconDatabaseSO _iconDatabase;
        private CurrencyFlightService _currencyFlightService;
        private IObjectPool _objectPool;
        
        private Dictionary<int, List<MapNodeDefinitionSO>> _buildingUnlocksByLevel;
        private readonly List<LevelRewardRowView> _spawnedRows = new List<LevelRewardRowView>();
        
        private readonly List<PendingFlightData> _pendingFlights = new List<PendingFlightData>();

        private IInputLockService _inputLockService;
        private bool _isPanelCurrentlyOpen;
        
        private readonly Queue<int> _pendingLevelUps = new Queue<int>();
        private bool _isLevelUpWindowActive;

        public void Initialize(
            IReadOnlyLevel levelModel, 
            LevelRewardSettingsSO rewardSettings,
            LevelProgressionSettingsSO progressionSettings,
            GlobalRewardIconDatabaseSO iconDatabase,
            RoadmapDatabaseSO roadmapDatabase,
            IInputLockService inputLockService,
            CurrencyFlightService currencyFlightService,
            IObjectPool objectPool)
        {
            _levelModel = levelModel;
            _rewardSettings = rewardSettings;
            _progressionSettings = progressionSettings;
            _iconDatabase = iconDatabase;
            _inputLockService = inputLockService;
            _currencyFlightService = currencyFlightService;
            _objectPool = objectPool;

            CacheBuildingUnlocks(roadmapDatabase);

            _hudView.OnOpenWindowClicked += HandleOpenWindowClicked;
            _windowView.OnCloseClicked += HandleCloseWindowClicked;
            _levelUpWindowView.OnCollectClicked += HandleCloseLevelUpWindowClicked;
            
            _levelModel.OnLevelChanged += HandleLevelChanged;
            _levelModel.OnExperienceChanged += HandleExperienceChanged;

            UpdateAllProgressVisuals();
        }

        private void CacheBuildingUnlocks(RoadmapDatabaseSO roadmapDatabase)
        {
            _buildingUnlocksByLevel = new Dictionary<int, List<MapNodeDefinitionSO>>();
            if (roadmapDatabase == null || roadmapDatabase.AllNodes == null) return;

            foreach (var node in roadmapDatabase.AllNodes)
            {
                int reqLevel = node.RequiredPlayerLevel; 
                if (reqLevel <= 1) continue;

                if (!_buildingUnlocksByLevel.ContainsKey(reqLevel))
                {
                    _buildingUnlocksByLevel[reqLevel] = new List<MapNodeDefinitionSO>();
                }
                _buildingUnlocksByLevel[reqLevel].Add(node);
            }
        }

        private void HandleOpenWindowClicked()
        {
            BuildRewardsList();
            _windowView.Show();
            if (!_isPanelCurrentlyOpen)
            {
                _inputLockService.AddLock();
                _isPanelCurrentlyOpen = true;
            }
        }

        private void HandleCloseWindowClicked()
        {
            _windowView.Hide();
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
                _isPanelCurrentlyOpen = false;
            }
        }
        
        private void HandleCloseLevelUpWindowClicked()
        {
            foreach (var flightData in _pendingFlights)
            {
                _currencyFlightService.PlayFlightAnimation(
                    flightData.Category, 
                    flightData.Amount, 
                    flightData.IconTransform.position
                );
            }
            _pendingFlights.Clear();
            _isLevelUpWindowActive = false;
            
            if (_pendingLevelUps.Count > 0)
            {
                ProcessNextLevelUpWindow();
            }
            else
            {
                _levelUpWindowView.HideLevelUpWindow();
                if (_isPanelCurrentlyOpen)
                {
                    _inputLockService.RemoveLock();
                    _isPanelCurrentlyOpen = false;
                }
            }
        }

        private void HandleLevelChanged(int newLevel)
        {
            UpdateAllProgressVisuals();
            _pendingLevelUps.Enqueue(newLevel);
            ProcessNextLevelUpWindow();
        }
        
        private void ProcessNextLevelUpWindow()
        {
            if (_isLevelUpWindowActive || _pendingLevelUps.Count == 0) return;

            int nextLevelToShow = _pendingLevelUps.Dequeue();
            
            bool windowOpened = TryOpenLevelUpWindow(nextLevelToShow);
            
            if (windowOpened)
            {
                _isLevelUpWindowActive = true;
            }
            else
            {
                ProcessNextLevelUpWindow();
            }
        }

        private void HandleExperienceChanged(int currentExp, int requiredExp) => UpdateAllProgressVisuals();

        private void UpdateAllProgressVisuals()
        {
            int currentLevel = _levelModel.CurrentLevel;
            int currentExp = _levelModel.CurrentExperience;
            int requiredExp = _progressionSettings.GetRequiredExperienceForLevel(currentLevel + 1);
            
            float levelExpRatio = 0f;
            if (requiredExp > 0)
            {
                levelExpRatio = (float)currentExp / requiredExp;
            }
            _hudView.UpdateHUD(currentLevel, currentExp, requiredExp);
            
            int maxLevel = _progressionSettings.MaxLevel;
            if (maxLevel <= 1) return; 
            float completedSteps = currentLevel - 1; 
            float totalContinuousProgress = completedSteps + levelExpRatio;
            float totalStepsToMax = maxLevel - 1;
            float pathSliderValue = totalContinuousProgress / totalStepsToMax;
            _windowView.UpdateProgress(pathSliderValue); 
        }
        
        private bool TryOpenLevelUpWindow(int newLevel)
        {
            if (_levelModel.CurrentLevel > 1)
            {
                foreach (Transform child in _levelUpWindowView.RewardsContainer)
                {
                    _objectPool.Despawn(PoolObjectType.LevelRewardItemView, child.gameObject);
                }
                
                _pendingFlights.Clear();
                _levelUpWindowView.SetLevelText(newLevel);

                var rewards = _rewardSettings.GetRewardForLevel(newLevel);
                bool hasStandardRewards = rewards != null && rewards.Count > 0;
                bool hasBuildingUnlock = _buildingUnlocksByLevel.TryGetValue(newLevel, out var unlockedBuildings);

                if (hasStandardRewards)
                {
                    foreach (var reward in rewards)
                    {
                        var itemView = CreateRewardItem(reward, _levelUpWindowView.RewardsContainer, PoolObjectType.LevelRewardItemView);
                        
                        if (reward.Category == Data.Quests.RewardCategory.Gold || 
                            reward.Category == Data.Quests.RewardCategory.Gem || 
                            reward.Category == Data.Quests.RewardCategory.Energy)
                        {
                            _pendingFlights.Add(new PendingFlightData
                            {
                                Category = reward.Category,
                                Amount = reward.Amount,
                                IconTransform = itemView.transform
                            });
                        }
                    }
                }
                
                if (hasBuildingUnlock)
                {
                    foreach (var building in unlockedBuildings)
                    {
                        CreateBuildingUnlockItem(building, _levelUpWindowView.RewardsContainer, PoolObjectType.LevelRewardItemView);
                    }
                }
                
                _levelUpWindowView.ShowLevelUpWindow();
                if (!_isPanelCurrentlyOpen)
                {
                    _inputLockService.AddLock();
                    _isPanelCurrentlyOpen = true;
                }
                return true;
            }
            return false;
        }

        private void BuildRewardsList()
        {
            if (_spawnedRows.Count > 0)
            {
                RefreshExistingRows();
                return;
            }

            for (int lvl = 1; lvl <= _progressionSettings.MaxLevel; lvl++)
            {
                LevelRewardRowView rowView = _objectPool.SpawnUI<LevelRewardRowView>(PoolObjectType.LevelRewardRowView, _windowView.ScrollContentParent);
                rowView.transform.SetAsFirstSibling();
                
                bool isCompleted = lvl <= _levelModel.CurrentLevel;
                bool isCurrent = lvl == _levelModel.CurrentLevel + 1;
                
                var rewards = _rewardSettings.GetRewardForLevel(lvl);
                bool hasStandardRewards = rewards != null && rewards.Count > 0;
                bool hasBuildingUnlock = _buildingUnlocksByLevel.TryGetValue(lvl, out var unlockedBuildings);
                
                bool isNoRewards = !hasStandardRewards && !hasBuildingUnlock;

                if (hasStandardRewards)
                {
                    foreach (var reward in rewards)
                    {
                        CreateRewardItem(reward, rowView.RewardsContainer, PoolObjectType.LevelRewardItemViewNoEffect);
                    }
                }
                
                if (hasBuildingUnlock)
                {
                    foreach (var building in unlockedBuildings)
                    {
                        CreateBuildingUnlockItem(building, rowView.RewardsContainer, PoolObjectType.LevelRewardItemViewNoEffect);
                    }
                }
                
                rowView.SetLevelInfo(lvl, isCompleted, isCurrent, isNoRewards);
                _spawnedRows.Add(rowView);
            }

            _windowView.SetFirstSiblingSlider();
        }

        private void RefreshExistingRows()
        {
            for (int i = 0; i < _spawnedRows.Count; i++)
            {
                int levelOfRow = i + 1; 
                bool isCompleted = levelOfRow <= _levelModel.CurrentLevel;
                bool isCurrent = levelOfRow == _levelModel.CurrentLevel + 1;
                
                var rewards = _rewardSettings.GetRewardForLevel(levelOfRow);
                bool hasStandardRewards = rewards != null && rewards.Count > 0;
                bool hasBuildingUnlock = _buildingUnlocksByLevel.ContainsKey(levelOfRow);
                bool isNoRewards = !hasStandardRewards && !hasBuildingUnlock;
                
                _spawnedRows[i].SetLevelInfo(levelOfRow, isCompleted, isCurrent, isNoRewards);
            }
        }

        private LevelRewardItemView CreateRewardItem(QuestRewardConfig reward, Transform container, PoolObjectType itemType)
        {
            LevelRewardItemView itemView = _objectPool.SpawnUI<LevelRewardItemView>(itemType, container);
            Sprite icon = reward.GetRewardIcon(_iconDatabase);
            string formattedText = FormatRewardText(reward);

            itemView.Setup(icon, formattedText);
            return itemView;
        }

        private LevelRewardItemView CreateBuildingUnlockItem(MapNodeDefinitionSO building, Transform container, PoolObjectType itemType)
        {
            LevelRewardItemView itemView = _objectPool.SpawnUI<LevelRewardItemView>(itemType, container);
            Sprite icon = building.Building != null ? building.Building.Levels[0].LevelSprite : null;
            string formattedText = building.Building.BuildingName; 

            itemView.Setup(icon, formattedText);
            return itemView;
        }

        private string FormatRewardText(QuestRewardConfig reward)
        {
            string itemName = reward.ItemDefinition != null ? reward.ItemDefinition.ItemName : "Unknown";

            switch (reward.Category)
            {
                case Data.Quests.RewardCategory.ItemSpawner:
                case Data.Quests.RewardCategory.Chest:
                    return $"{reward.ItemDefinition.ItemName} x{reward.Amount}";
                case Data.Quests.RewardCategory.Gem:
                case Data.Quests.RewardCategory.Gold:
                case Data.Quests.RewardCategory.Energy:
                    return $"x{reward.Amount}";
                default:
                    return $"x{reward.Amount}";
            }
        }

        public void OnDestroy()
        {
            if (_isPanelCurrentlyOpen && _inputLockService != null)
            {
                _inputLockService.RemoveLock();
            }
            if (_hudView != null) _hudView.OnOpenWindowClicked -= HandleOpenWindowClicked;
            if (_windowView != null) _windowView.OnCloseClicked -= HandleCloseWindowClicked;
            if (_levelUpWindowView != null) _levelUpWindowView.OnCollectClicked -= HandleCloseLevelUpWindowClicked;
            
            if (_levelModel != null)
            {
                _levelModel.OnLevelChanged -= HandleLevelChanged;
                _levelModel.OnExperienceChanged -= HandleExperienceChanged;
            }
        }
    }
}