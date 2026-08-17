using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Data.Roadmap;
using Utils;

namespace UI.Map
{
    public class BuildingUpgradePopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _buildingUpgradePopupPrefab;
        [Header("Information Textures")]
        [SerializeField] private TextMeshProUGUI _buildingNameText;
        [SerializeField] private TextMeshProUGUI _levelProgressText;
        [SerializeField] private TextMeshProUGUI _goldProgressText;
        [SerializeField] private Transform _rewardsContainer;
        [SerializeField] private Image _buildingIcon;
        [SerializeField] private Image _goldProgressBackground;
        [SerializeField] private Color _cantAffordColor;
        [SerializeField] private Color _canAffordColor;

        [Header("Controls")]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _notEnoughGoldMessage;
        [SerializeField] private UIPopupAnimator _popupAnimator;


        public Transform RewardsContainer => _rewardsContainer;

        public event Action OnUpgradeClicked;
        public event Action OnCloseClicked;

        private void Awake()
        {
            _upgradeButton.onClick.AddListener(() => OnUpgradeClicked?.Invoke());
            _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void Open()
        {
            _buildingUpgradePopupPrefab.SetActive(true);
            _popupAnimator.Show();
        }

        public void Close()
        {
            _popupAnimator.Hide(() =>
            {
                _buildingUpgradePopupPrefab.SetActive(false);
            });
        }

        public void SetContent(
            string name, 
            string progress, 
            string goldProgress,
            Sprite icon, 
            bool canAfford, 
            bool isMaxLevel)
        {
            _buildingNameText.text = name;
            
            if (isMaxLevel)
            {
                _levelProgressText.text = $"MAX Level!";
                _upgradeButton.interactable = false;
                _notEnoughGoldMessage.SetActive(false);
                _goldProgressBackground.gameObject.SetActive(false);
                return;
            }
            
            _goldProgressBackground.gameObject.SetActive(true);
            
            _levelProgressText.text = progress;
            _buildingIcon.sprite = icon;
            UpdateGoldProgressArea(canAfford, goldProgress);
            
            _upgradeButton.interactable = canAfford;
            _notEnoughGoldMessage.SetActive(!canAfford);
        }

        private void UpdateGoldProgressArea(bool canAfford, string goldProgress)
        {
            _goldProgressBackground.color = canAfford ? _canAffordColor : _cantAffordColor;
            _goldProgressText.text = goldProgress;
        }

        private void OnDestroy()
        {
            _upgradeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }
    }
}