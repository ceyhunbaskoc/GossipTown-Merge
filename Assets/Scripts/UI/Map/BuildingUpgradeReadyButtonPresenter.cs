using System;
using Core.Quests;
using Core.Services;
using Data.Quests;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Map
{
    public class BuildingUpgradeReadyButtonPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private AnimatedLayoutElement _backToMenuButtonAnimatedLayoutElement;
        [SerializeField] private GameObject _buttonRoot;

        private RoadmapProgressionService _roadmapProgressionService;

        
        public event Action OnBackToMenuRequested;

        public void Initialize(RoadmapProgressionService roadmapProgressionService)
        {
            _roadmapProgressionService = roadmapProgressionService;
            _backToMenuButton.onClick.AddListener(() => OnBackToMenuRequested?.Invoke());

            _roadmapProgressionService.OnAnyBuildingUpgradeCheck += EvaluateVisibility;
            _roadmapProgressionService.CheckAnyNodeUpgradeable();
        }


        private void EvaluateVisibility(bool shouldOpen)
        {
            if (shouldOpen)
            {
                _backToMenuButtonAnimatedLayoutElement.Show();
            }
            else
            {
                _backToMenuButtonAnimatedLayoutElement.Hide();
            }
        }

        private void OnDestroy()
        {
            _backToMenuButton.onClick.RemoveAllListeners();

        }
    }
}