using System;
using Core.Quests;
using UnityEngine;
using UnityEngine.UI;
using Data.Quests;
using UI.Components;

namespace UI.Quests
{
    public class QuestNotificationButtonPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _questButton;
        [SerializeField] private AnimatedLayoutElement _questButtonAnimatedLayoutElement;
        [SerializeField] private GameObject _buttonRoot;

        private WeeklyQuestService _questService;
        
        public event Action OnOpenPanelRequested;

        public void Initialize(WeeklyQuestService questService)
        {
            _questService = questService;

            _questButton.onClick.AddListener(() => OnOpenPanelRequested?.Invoke());

            _questService.OnQuestCompleted += HandleQuestStateChanged;
            _questService.OnQuestRewardClaimed += HandleQuestStateChanged;

            EvaluateVisibility();
        }

        private void HandleQuestStateChanged(QuestDefinitionSO questDef)
        {
            EvaluateVisibility();
        }

        private void EvaluateVisibility()
        {
            bool hasPendingRewards = _questService.HasAnyUnclaimedCompletedQuests();
            if (hasPendingRewards)
            {
                _questButtonAnimatedLayoutElement.Show();
            }
            else
            {
                _questButtonAnimatedLayoutElement.Hide();
            }
        }

        private void OnDestroy()
        {
            _questButton.onClick.RemoveAllListeners();

            if (_questService != null)
            {
                _questService.OnQuestCompleted -= HandleQuestStateChanged;
                _questService.OnQuestRewardClaimed -= HandleQuestStateChanged;
            }
        }
    }
}