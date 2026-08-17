using System;
using Core.Reward;
using Data.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Login
{
    public class DailyLoginItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _dayText;
        
        [Header("State Overlays")]
        [SerializeField] private GameObject _claimableHighlight;
        [SerializeField] private GameObject _claimedCheckmark;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private Button _claimButton;

        private int _absoluteDayIndex;
        public event Action<int> OnClaimClicked;

        private void Awake()
        {
            _claimButton.onClick.AddListener(() => OnClaimClicked?.Invoke(_absoluteDayIndex));
        }

        public void Initialize(int absoluteDayIndex, QuestRewardConfig rewardConfig, string dayLabel)
        {
            _absoluteDayIndex = absoluteDayIndex;
            _dayText.text = dayLabel;
        }

        public void SetStateReadyToClaim()
        {
            _claimableHighlight.SetActive(true);
            _claimedCheckmark.SetActive(false);
            _lockedOverlay.SetActive(false);
            
            _claimButton.gameObject.SetActive(true);
            _claimButton.interactable = true;
        }

        public void SetStateClaimed()
        {
            _claimableHighlight.SetActive(false);
            _claimedCheckmark.SetActive(true);
            _lockedOverlay.SetActive(false);
            
            _claimButton.gameObject.SetActive(false);
        }

        public void SetStateLocked()
        {
            _claimableHighlight.SetActive(false);
            _claimedCheckmark.SetActive(false);
            _lockedOverlay.SetActive(true);
            
            _claimButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _claimButton.onClick.RemoveAllListeners();
        }
    }
}