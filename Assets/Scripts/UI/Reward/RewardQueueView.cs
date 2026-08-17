using System.Collections;
using Core.GridSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Reward;
using Data;
using UI.Components;

namespace UI.Rewards
{
    public class RewardQueueView : MonoBehaviour
    {
        [SerializeField] private Button _claimButton;
        [SerializeField] private AnimatedLayoutElement _claimButtonAnimatedLayoutElement;
        [SerializeField] private Image _rewardIconImage;
        [SerializeField] private TextMeshProUGUI _badgeText;

        private PendingRewardModel _rewardModel;
        private RewardPlacementController _placementController;
        private ItemDatabaseSO _itemDatabase;
        private IWarningMessageService _warningMessageService;

        private bool _isButtonActive = false;

        public void Initialize(PendingRewardModel rewardModel, RewardPlacementController placementController, ItemDatabaseSO itemDatabase
        ,IWarningMessageService warningMessageService)
        {
            _rewardModel = rewardModel;
            _placementController = placementController;
            _itemDatabase = itemDatabase;
            _warningMessageService = warningMessageService;

            _claimButton.onClick.AddListener(OnClaimClicked);

            _rewardModel.OnRewardAdded += RefreshUI;
            _rewardModel.OnRewardClaimed += OnRewardClaimedLocally;

            RefreshUI(default);
        }

        private void OnClaimClicked()
        {
            if (!_placementController.OnRewardUIActionClicked())
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(_claimButton.transform.position);
                _warningMessageService.ShowWarning("Grid is Full!", screenPos);
            }
        }

        private void OnRewardClaimedLocally(ItemIdentifier claimedItem)
        {
            RefreshUI(default);
        }

        private void RefreshUI(ItemIdentifier dummy)
        {
            if (_rewardModel.TryPeekNextReward(out ItemIdentifier nextReward))
            {
                if (!_isButtonActive)
                {
                    _claimButtonAnimatedLayoutElement.Show();
                    _isButtonActive = true;
                }
                BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(nextReward.Id);
                _rewardIconImage.sprite = itemDef.GetIcon(nextReward.Level);

                int count = _rewardModel.PendingCount;
                _badgeText.text = count > 1 ? count.ToString() : string.Empty;
                _badgeText.gameObject.SetActive(count > 1);
            }
            else
            {
                _claimButtonAnimatedLayoutElement.Hide();
                _isButtonActive = false;
            }
        }

        private void OnDestroy()
        {
            if (_rewardModel != null)
            {
                _rewardModel.OnRewardAdded -= RefreshUI;
                _rewardModel.OnRewardClaimed -= OnRewardClaimedLocally;
            }
            _claimButton.onClick.RemoveListener(OnClaimClicked);
        }
    }
}