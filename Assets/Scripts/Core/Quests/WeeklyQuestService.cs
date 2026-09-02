using System;
using System.Collections.Generic;
using Core.Economy;
using Core.Milestones;
using UnityEngine;
using Core.SaveSystem;
using Data.Quests;
using Core.Reward;
using Core.Services;

namespace Core.Quests
{
    public class WeeklyQuestService
    {
        public event Action<QuestDefinitionSO, int, int> OnQuestProgressChanged; 
        public event Action<QuestDefinitionSO> OnQuestCompleted;
        public event Action<QuestDefinitionSO> OnQuestRewardClaimed;

        private readonly WeeklyQuestDatabaseSO _questDatabase;
        private readonly RewardDispatcherService _rewardDispatcher;
        private readonly RewardSelectionService _rewardSelectionService;
        private readonly IEconomyModifier _economyModifier;
        private readonly ITimeManager _timeManager;
        private readonly MilestoneService _milestoneService;
        
        private WeeklyQuestSaveData _currentSaveData;
        private WeeklyQuestConfigSO _activeWeekConfig;
        
        private readonly Dictionary<string, QuestProgressData> _progressMap;
        private readonly Dictionary<string, QuestDefinitionSO> _activeQuestsMap;
        
        public int CurrentActiveDay => _currentSaveData != null ? _currentSaveData.CurrentActiveDayIndex : 1;

        public WeeklyQuestService(
            WeeklyQuestDatabaseSO questDatabase, 
            RewardDispatcherService rewardDispatcher,
            RewardSelectionService rewardSelectionService,
            IEconomyModifier economyModifier,
            ITimeManager timeManager,
            MilestoneService milestoneService)
        {
            _questDatabase = questDatabase;
            _rewardDispatcher = rewardDispatcher;
            _rewardSelectionService = rewardSelectionService;
            _economyModifier = economyModifier;
            _timeManager = timeManager;
            _milestoneService = milestoneService;
            _progressMap = new Dictionary<string, QuestProgressData>();
            _activeQuestsMap = new Dictionary<string, QuestDefinitionSO>();
            
            LoadSaveData(null); 
        }
        
        public IEnumerable<QuestDefinitionSO> GetActiveQuests()
        {
            return _activeQuestsMap.Values;
        }
        public IReadOnlyDictionary<string, QuestProgressData> GetProgressData()
        {
            return _progressMap;
        }
        
        public bool HasAnyUnclaimedCompletedQuests()
        {
            foreach (var progress in _progressMap.Values)
            {
                if (progress.IsCompleted && !progress.IsRewardClaimed)
                {
                    return true;
                }
            }
            return false;
        }
        
        public WeeklyQuestSaveData GetSaveData()
        {
            if (_currentSaveData == null)
            {
                Debug.LogWarning("[WeeklyQuestService] GetSaveData çağrıldı ancak _currentSaveData null! Boş veri döndürülüyor.");
                return new WeeklyQuestSaveData();
            }

            return _currentSaveData;
        }

        public void LoadSaveData(WeeklyQuestSaveData savedData)
        {
            _progressMap.Clear();
            _activeQuestsMap.Clear();

            _currentSaveData = savedData;

            if (_currentSaveData == null || string.IsNullOrEmpty(_currentSaveData.ActiveWeekId))
            {
                _activeWeekConfig = _questDatabase.GetDefaultStarterWeek();
                
                _currentSaveData = new WeeklyQuestSaveData 
                { 
                    ActiveWeekId = _activeWeekConfig.WeekId,
                    CurrentActiveDayIndex = 1,
                    QuestProgressList = new List<QuestProgressData>()
                };
            }
            else
            {
                _activeWeekConfig = _questDatabase.GetWeekById(_currentSaveData.ActiveWeekId);
                
                if (_activeWeekConfig == null)
                {
                    Debug.LogError($"[WeeklyQuestService] Saved week ID '{_currentSaveData.ActiveWeekId}' not found in database! Falling back to default.");
                    _activeWeekConfig = _questDatabase.GetDefaultStarterWeek();
                    _currentSaveData.ActiveWeekId = _activeWeekConfig.WeekId;
                }
            }

            foreach (var progress in _currentSaveData.QuestProgressList)
            {
                _progressMap[progress.QuestId] = progress;
            }

            InitializeUnlockedQuests();
            PerformBootTimeCheck();
        }
        
        private void PerformBootTimeCheck()
        {
            DateTime currentTime = new DateTime(_timeManager.CurrentTimeTicks);
            CheckAndApplyDayRollover(currentTime);

            ScheduleNextMidnightRollover(currentTime);
        }

        private void ScheduleNextMidnightRollover(DateTime currentTime)
        {
            DateTime nextMidnight = currentTime.Date.AddDays(1);
            
            var midnightTracker = new MidnightTimer(nextMidnight.Ticks, () =>
            {
                DateTime exactMidnightTime = new DateTime(_timeManager.CurrentTimeTicks);
                CheckAndApplyDayRollover(exactMidnightTime);
                ScheduleNextMidnightRollover(exactMidnightTime);
            });
            _timeManager.RegisterTimer(midnightTracker);
        }
        
        public void CheckAndApplyDayRollover(DateTime currentNormalizedTime)
        {
            if (_currentSaveData == null || _activeWeekConfig == null) return;

            if (_currentSaveData.LastDayUpdateTimestampTicks == 0)
            {
                _currentSaveData.LastDayUpdateTimestampTicks = currentNormalizedTime.Date.Ticks;
                return;
            }

            DateTime lastUpdate = new DateTime(_currentSaveData.LastDayUpdateTimestampTicks);
            int daysPassed = (currentNormalizedTime.Date - lastUpdate.Date).Days;

            if (daysPassed > 0)
            {
                int targetDayIndex = _currentSaveData.CurrentActiveDayIndex + daysPassed;

                if (targetDayIndex > 7)
                {
                    int weeksPassed = targetDayIndex / 7;
                    int remainingDays = targetDayIndex % 7;
                    
                    if (remainingDays == 0) 
                    {
                        weeksPassed--;
                        remainingDays = 7;
                    }

                    TransitionToNextWeek(weeksPassed);
                    _currentSaveData.CurrentActiveDayIndex = remainingDays;
                }
                else
                {
                    _currentSaveData.CurrentActiveDayIndex = targetDayIndex;
                }

                _currentSaveData.LastDayUpdateTimestampTicks = currentNormalizedTime.Date.Ticks;
                InitializeUnlockedQuests();
            }
        }
        
        private void TransitionToNextWeek(int weeksToAdvance)
        {
            int currentIndex = _questDatabase.AllWeeks.FindIndex(w => w.WeekId == _activeWeekConfig.WeekId);
            if (currentIndex == -1) currentIndex = 0;
            int nextIndex = Mathf.Min(currentIndex + weeksToAdvance, _questDatabase.AllWeeks.Count - 1);
            
            _activeWeekConfig = _questDatabase.AllWeeks[nextIndex];
            _currentSaveData.ActiveWeekId = _activeWeekConfig.WeekId;

            _progressMap.Clear();
            _activeQuestsMap.Clear();
            _currentSaveData.QuestProgressList.Clear();
        }
        
        public DailyQuestGroup GetDailyQuestGroup(int dayIndex)
        {
            return _activeWeekConfig?.DailyGroups.Find(g => g.DayIndex == dayIndex);
        }


        private void InitializeUnlockedQuests()
        {
            for (int day = 1; day <= _currentSaveData.CurrentActiveDayIndex; day++)
            {
                var dayGroup = _activeWeekConfig.DailyGroups.Find(g => g.DayIndex == day);
                if (dayGroup == null) continue;

                foreach (var questDef in dayGroup.Quests)
                {
                    _activeQuestsMap[questDef.QuestId] = questDef;

                    if (!_progressMap.ContainsKey(questDef.QuestId))
                    {
                        var newProgress = new QuestProgressData 
                        { 
                            QuestId = questDef.QuestId, 
                            CurrentAmount = 0 
                        };
                        _progressMap[questDef.QuestId] = newProgress;
                        _currentSaveData.QuestProgressList.Add(newProgress);
                    }
                }
            }
        }

        public void ProcessAction(QuestType actionType, int amount, string itemId = "")
        {
            foreach (var kvp in _activeQuestsMap)
            {
                var questDef = kvp.Value;
                
                if (questDef.Type != actionType) continue;
                
                if (!string.IsNullOrEmpty(questDef.RequiredItemId) && questDef.RequiredItemId != itemId) continue;

                var progress = _progressMap[questDef.QuestId];
                
                if (progress.IsCompleted) continue;

                progress.CurrentAmount += amount;
                
                if (progress.CurrentAmount >= questDef.TargetAmount)
                {
                    progress.CurrentAmount = questDef.TargetAmount;
                    progress.IsCompleted = true;
                    OnQuestCompleted?.Invoke(questDef);
                }
                else
                {
                    OnQuestProgressChanged?.Invoke(questDef, progress.CurrentAmount, questDef.TargetAmount);
                }
            }
        }

        public void ClaimQuestReward(string questId)
        {
            if (!_activeQuestsMap.TryGetValue(questId, out var questDef)) return;
            if (!_progressMap.TryGetValue(questId, out var progress)) return;

            if (!progress.IsCompleted || progress.IsRewardClaimed)
            {
                Debug.LogWarning($"[WeeklyQuest] Illegal claim attempt for quest: {questId}");
                return;
            }

            foreach (var rewardConfig in questDef.Rewards)
            {
                if (rewardConfig.Category == RewardCategory.Chest)
                {
                    RewardPayload payload = rewardConfig.ToPayload();
                    
                    if (payload.IsValid)
                    {
                        _rewardDispatcher.DispatchReward(payload);
                    }
                }
                else if (rewardConfig.Category == RewardCategory.ItemSpawner)
                {
                    RewardPayload? payload = _rewardSelectionService.TryGetSpawnerUpgradeReward();
                    if (payload.HasValue)
                    {
                        _rewardDispatcher.DispatchReward(payload.Value);
                    }
                }
                else
                {
                    _rewardDispatcher.DispatchCurrencyReward(rewardConfig);
                }
            }

            progress.IsRewardClaimed = true;
            _milestoneService.AddMedals(questDef.MedalReward);
            OnQuestRewardClaimed?.Invoke(questDef);
        }
    }
}