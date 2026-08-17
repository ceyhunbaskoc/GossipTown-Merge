using System.Collections.Generic;
using Core.PoolSystem;
using Core.Quests;
using UnityEngine;
using Core.Services;
using Data.Quests;
using Data.Reward;
using UI.FlightSystem;
using UnityEngine.UI;
using Utils;

namespace UI.Quests
{
    public class WeeklyQuestPanelPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _questContentContainer;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _panelCloseButton;
        [SerializeField] private Button _panelOpenButton;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        [Header("Day Tabs")]
        [Tooltip("Assign 7 buttons representing Day 1 to Day 7 sequentially.")]
        [SerializeField] private Button[] _dayTabButtons; 

        private WeeklyQuestService _questService;
        private int _selectedDayIndex = 1;
        
        private IInputLockService _inputLockService;
        private bool _isPanelCurrentlyOpen;
        
        private GlobalRewardIconDatabaseSO _iconDatabase;
        private CurrencyFlightService _currencyFlightService;
        private IObjectPool _poolService;
        
        private readonly Dictionary<string, QuestItemView> _activeViews = new Dictionary<string, QuestItemView>();

        public void Initialize(
            WeeklyQuestService questService, 
            IInputLockService inputLockService, 
            GlobalRewardIconDatabaseSO iconDatabase,
            CurrencyFlightService currencyFlightService,
            IObjectPool poolService)
        {
            _questService = questService;
            _inputLockService = inputLockService;
            _iconDatabase = iconDatabase;
            _currencyFlightService = currencyFlightService;
            _poolService = poolService;

            _questService.OnQuestProgressChanged += HandleQuestProgressChanged;
            _questService.OnQuestCompleted += HandleQuestCompleted;
            _questService.OnQuestRewardClaimed += HandleQuestRewardClaimed;
            
            _panelCloseButton.onClick.AddListener(ClosePanel);
            _panelOpenButton.onClick.AddListener(OpenPanel);

            for (int i = 0; i < _dayTabButtons.Length; i++)
            {
                int dayIndex = i + 1;
                _dayTabButtons[i].onClick.AddListener(() => SelectDayTab(dayIndex));
            }

            _panelRoot.SetActive(false);
        }

        public void OpenPanel()
        {
            if (!_isPanelCurrentlyOpen)
            {
                _inputLockService.AddLock();
                _isPanelCurrentlyOpen = true;
            }
            _selectedDayIndex = _questService.CurrentActiveDay;
            SelectDayTab(_selectedDayIndex);
            
            _panelRoot.SetActive(true);
            _popupAnimator.Show();
        }

        private void ClosePanel()
        {
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
                _isPanelCurrentlyOpen = false;
            }
            _popupAnimator.Hide(() =>
            {
                _panelRoot.SetActive(false);
            });
        }

        private void SelectDayTab(int dayIndex)
        {
            _selectedDayIndex = dayIndex;
            PopulatePanel();
        }

        private void PopulatePanel()
        {
            ClearExistingViews();

            var dailyGroup = _questService.GetDailyQuestGroup(_selectedDayIndex);
            if (dailyGroup == null) return;

            bool isDayLocked = _selectedDayIndex > _questService.CurrentActiveDay;
            var progressData = _questService.GetProgressData(); 

            foreach (var questDef in dailyGroup.Quests)
            {
                var viewInstance = _poolService.Spawn<QuestItemView>(PoolObjectType.QuestItem, Vector3.zero, Quaternion.identity, _questContentContainer);
                
                string questTitle = GetLocalizedQuestTitle(questDef); 
                List<RewardDisplayData> displayRewards = GenerateRewardDisplayData(questDef);

                if (isDayLocked)
                {
                    viewInstance.Initialize(questDef.QuestId, questTitle, 0, questDef.TargetAmount, false, false, displayRewards, _poolService);
                    viewInstance.SetLockedState();
                }
                else
                {
                    if (progressData.TryGetValue(questDef.QuestId, out var progress))
                    {
                        viewInstance.Initialize(
                            questDef.QuestId, 
                            questTitle, 
                            progress.CurrentAmount, 
                            questDef.TargetAmount, 
                            progress.IsCompleted, 
                            progress.IsRewardClaimed,
                            displayRewards,
                            _poolService
                        );

                        viewInstance.OnClaimClicked += _questService.ClaimQuestReward;
                    }
                }
                viewInstance.transform.localScale = Vector3.one;
                viewInstance.transform.localPosition = Vector3.zero;

                _activeViews[questDef.QuestId] = viewInstance;
            }
        }

        private List<RewardDisplayData> GenerateRewardDisplayData(QuestDefinitionSO questDef)
        {
            List<RewardDisplayData> rewardVisuals = new List<RewardDisplayData>();

            if (questDef.MedalReward > 0 && _iconDatabase.MedalIcon != null)
            {
                rewardVisuals.Add(new RewardDisplayData 
                { 
                    Icon = _iconDatabase.MedalIcon, 
                    AmountText = questDef.MedalReward.ToString() 
                });
            }

            if (questDef.Rewards != null)
            {
                foreach (var reward in questDef.Rewards)
                {
                    if (reward == null) continue;

                    rewardVisuals.Add(new RewardDisplayData
                    {
                        Icon = reward.GetRewardIcon(_iconDatabase),
                        AmountText = reward.Amount.ToString()
                    });
                }
            }

            return rewardVisuals;
        }

        private void HandleQuestProgressChanged(QuestDefinitionSO questDef, int current, int target)
        {
            if (!_panelRoot.activeSelf) return;
            if (_activeViews.TryGetValue(questDef.QuestId, out var view))
            {
                view.UpdateProgress(current, target);
            }
        }

        private void HandleQuestCompleted(QuestDefinitionSO questDef)
        {
            if (!_panelRoot.activeSelf) return;
            if (_activeViews.TryGetValue(questDef.QuestId, out var view))
            {
                view.UpdateProgress(questDef.TargetAmount, questDef.TargetAmount);
                view.SetCompletedState();
            }
        }

        private void HandleQuestRewardClaimed(QuestDefinitionSO questDef)
        {
            foreach (var rewardConfig in questDef.Rewards)
            {
                if (rewardConfig.Category == RewardCategory.Gold || 
                    rewardConfig.Category == RewardCategory.Gem || 
                    rewardConfig.Category == RewardCategory.Energy)
                {
                    if (_activeViews.TryGetValue(questDef.QuestId, out QuestItemView clickedView))
                    {
                        _currencyFlightService.PlayFlightAnimation(
                            rewardConfig.Category, 
                            rewardConfig.Amount, 
                            clickedView.transform.position); 
                    }
                }
            }
            
            if (_activeViews.TryGetValue(questDef.QuestId, out var view))
            {
                view.SetClaimedState();
            }
        }

        private string GetLocalizedQuestTitle(QuestDefinitionSO questDef)
        {
            switch (questDef.Type)
            {
                case QuestType.MergeItem: return $"Merge items {questDef.TargetAmount} times.";
                case QuestType.UpgradeBuilding: return $"Upgrade a building {questDef.TargetAmount} times.";
                default: return "Complete the quest.";
            }
        }

        private void ClearExistingViews()
        {
            foreach (var kvp in _activeViews)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.OnClaimClicked -= _questService.ClaimQuestReward;
                    
                    _poolService.Despawn<QuestItemView>(PoolObjectType.QuestItem, kvp.Value);
                }
            }
            _activeViews.Clear();
        }

        private void OnDestroy()
        {
            if (_isPanelCurrentlyOpen && _inputLockService != null)
            {
                _inputLockService.RemoveLock();
            }
            if (_questService != null)
            {
                _questService.OnQuestProgressChanged -= HandleQuestProgressChanged;
                _questService.OnQuestCompleted -= HandleQuestCompleted;
                _questService.OnQuestRewardClaimed -= HandleQuestRewardClaimed;
            }
            
            _panelCloseButton.onClick.RemoveListener(ClosePanel);
            _panelOpenButton.onClick.RemoveListener(OpenPanel);
            foreach (var btn in _dayTabButtons)
            {
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                }
            }
            
            ClearExistingViews();
        }
    }
}