using System;
using Core.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementSlotView : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private Button _claimButton;
    [SerializeField] private TextMeshProUGUI _rewardAmountText;

    private Action _onClaimClicked;

    private void Awake()
    {
        _claimButton.onClick.AddListener(() => _onClaimClicked?.Invoke());
    }

    public void Bind(AchievementSlotViewModel viewModel, Action onClaimAction)
    {
        _onClaimClicked = onClaimAction;
            
        _iconImage.sprite = viewModel.Icon; 
            
        if (_rewardAmountText != null)
            _rewardAmountText.text = viewModel.RewardAmount.ToString();

        switch (viewModel.State)
        {
            case AchievementState.Locked:
                _lockedOverlay.SetActive(true);
                _claimButton.gameObject.SetActive(false);
                _iconImage.color = Color.black;
                break;
            case AchievementState.UnlockedUnclaimed:
                _lockedOverlay.SetActive(false);
                _claimButton.gameObject.SetActive(true);
                _iconImage.color = Color.white;
                break;
            case AchievementState.Claimed:
                _lockedOverlay.SetActive(false);
                _claimButton.gameObject.SetActive(false);
                _iconImage.color = Color.white;
                break;
        }
    }
}