using System;
using System.Collections.Generic;
using Core.PoolSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Quests
{
    public class QuestItemView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider _progressBarSlider;
        [SerializeField] private Button _claimButton;
        
        [Header("Rewards Setup")]
        [SerializeField] private Transform _rewardsContainer;
        
        private string _questId;
        private IObjectPool _objectPool;
        private readonly List<QuestRewardIconView> _spawnedRewardViews = new List<QuestRewardIconView>();
        
        public event Action<string> OnClaimClicked;

        private void Awake()
        {
            _claimButton.onClick.AddListener(() => OnClaimClicked?.Invoke(_questId));
        }

        public void Initialize(
            string questId, 
            string title, 
            int currentProgress, 
            int targetProgress, 
            bool isCompleted, 
            bool isClaimed,
            List<RewardDisplayData> rewardsToDisplay,
            IObjectPool objectPool)
        {
            _questId = questId;
            _titleText.text = title;
            _objectPool = objectPool;
            
            UpdateProgress(currentProgress, targetProgress);
            GenerateRewardIcons(rewardsToDisplay);

            if (isClaimed)
            {
                SetClaimedState();
            }
            else if (isCompleted)
            {
                SetCompletedState();
            }
            else
            {
                SetInProgressState();
            }
        }

        private void GenerateRewardIcons(List<RewardDisplayData> rewards)
        {
            foreach (var view in _spawnedRewardViews)
            {
                if (view != null) 
                    _objectPool.Despawn(PoolObjectType.QuestRewardItem, view);
            }
            _spawnedRewardViews.Clear();

            if (rewards == null) return;

            foreach (var rewardData in rewards)
            {
                QuestRewardIconView iconInstance = _objectPool.Spawn<QuestRewardIconView>(PoolObjectType.QuestRewardItem, Vector3.zero, Quaternion.identity, _rewardsContainer);
                iconInstance.Initialize(rewardData.Icon, rewardData.AmountText);
                _spawnedRewardViews.Add(iconInstance);
            }
        }

        public void UpdateProgress(int current, int target)
        {
            if (target <= 0) target = 1;
            
            float fillAmount = Mathf.Clamp01((float)current / target);
            _progressBarSlider.value = fillAmount;
            _progressText.text = $"{current} / {target}";
        }

        public void SetInProgressState()
        {
            _claimButton.gameObject.SetActive(false);
        }

        public void SetCompletedState()
        {
            _claimButton.gameObject.SetActive(true);
            _claimButton.interactable = true;
        }

        public void SetClaimedState()
        {
            _claimButton.gameObject.SetActive(false);
            
            _progressText.text = "COMPLETED";
        }
        
        public void SetLockedState()
        {
            _claimButton.gameObject.SetActive(false);
            _progressBarSlider.value = 0;
            _progressText.text = "LOCKED";
        }

        private void OnDestroy()
        {
            _claimButton.onClick.RemoveAllListeners();
        }
    }
}