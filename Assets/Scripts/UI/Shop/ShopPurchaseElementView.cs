using System;
using Core.PoolSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopPurchaseElementView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _adCountText;
        [SerializeField] private TextMeshProUGUI _rewardValue;
        [SerializeField] private Button _purchaseButton;

        public event Action OnPurchaseButtonClicked;

        private void Awake()
        {
            _purchaseButton.onClick.AddListener(_purchaseButtonClicked);
        }

        public void Setup(string title, Sprite icon, int watchedAdCount, int requiredAdCount, int rewardValue)
        {
            _titleText.text = title;
            _iconImage.sprite = icon;
            int watchedCount = Mathf.Min(watchedAdCount, requiredAdCount);
            _adCountText.text = $"{watchedCount}/{requiredAdCount}";
            _purchaseButton.interactable = watchedCount < requiredAdCount;
            _rewardValue.text = rewardValue.ToString();
        }

        private void _purchaseButtonClicked()
        {
            _purchaseButton.interactable = false;
            OnPurchaseButtonClicked?.Invoke();
        }

        public void HandleAdCompleted()
        {
            _purchaseButton.interactable = true;
        }

        public void HandleAnyAdRequested()
        {
            _purchaseButton.interactable = false;
        }

        public void HandleAnyAdCompleted()
        {
            _purchaseButton.interactable = true;
        }

        private void OnDestroy()
        {
            _purchaseButton.onClick.RemoveListener(_purchaseButtonClicked);

        }

        public void OnSpawned()
        {}

        public void OnDespawned()
        {
            OnPurchaseButtonClicked = null;
            
        }
    }
}