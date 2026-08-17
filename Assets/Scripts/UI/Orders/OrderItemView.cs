using System;
using Core.PoolSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Orders
{
    public class OrderItemView : MonoBehaviour, IPoolable
    {
        [Header("Visual Elements")]
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Color _defaultBackgroundColor;
        [SerializeField] private Button _itemButton;
        
        private int _requiredCount;
        private string _itemId;
        private int _itemLevel;

        public event Action<string, int> OnOrderItemClicked;

        public void Setup(Sprite icon, int currentCount, int requiredCount, string itemId, int itemLevel)
        {
            _itemIcon.sprite = icon;
            _requiredCount = requiredCount;
            _backgroundImage.color = _defaultBackgroundColor;
            _itemId = itemId;
            _itemLevel = itemLevel;
            UpdateCurrentCount(currentCount);
            _itemButton.onClick.AddListener(_onClicked);
        }

        private void _onClicked()
        {
            OnOrderItemClicked?.Invoke(_itemId, _itemLevel);
        }

        public void UpdateCurrentCount(int currentCount)
        {
            _countText.text = $"{currentCount}/{_requiredCount}";
            _backgroundImage.color = currentCount >= _requiredCount ? Color.green : _defaultBackgroundColor;
        }
        
        public void OnSpawned()
        {
            _countText.text = string.Empty;
        }

        public void OnDespawned()
        {
            _itemIcon.sprite = null;
            _itemId = null;
            _itemButton.onClick.RemoveListener(_onClicked);
        }
    }
}