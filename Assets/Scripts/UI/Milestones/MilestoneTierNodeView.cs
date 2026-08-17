using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Milestones
{
    public class MilestoneTierNodeView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _requiredMedalText;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private Button _claimButton;
        
        [Header("State Visuals")]
        [SerializeField] private GameObject _readyToClaimVisual;
        [SerializeField] private GameObject _claimedStateVisual;
        
        private string _tierId;
        public event Action<string> OnNodeClicked;

        private void Awake()
        {
            _claimButton.onClick.AddListener(() => OnNodeClicked?.Invoke(_tierId));
        }

        public void Initialize(string tierId, int requiredMedals, Sprite rewardSprite)
        {
            _tierId = tierId;
            _requiredMedalText.text = requiredMedals.ToString();
            
            if (rewardSprite != null && _rewardIcon != null)
            {
                _rewardIcon.sprite = rewardSprite;
            }
        }

        public void SetStateLocked()
        {
            _readyToClaimVisual.SetActive(false);
            _claimedStateVisual.SetActive(false);
            
            _claimButton.interactable = false;
        }

        public void SetStateReady()
        {
            _readyToClaimVisual.SetActive(true);
            _claimedStateVisual.SetActive(false);
            
            _claimButton.interactable = true;
        }

        public void SetStateClaimed()
        {
            _readyToClaimVisual.SetActive(false);
            _claimedStateVisual.SetActive(true);
            
            _claimButton.interactable = false;
        }

        private void OnDestroy()
        {
            _claimButton.onClick.RemoveAllListeners();
        }
    }
}