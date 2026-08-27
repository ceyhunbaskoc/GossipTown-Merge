using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Core.GridSystem;
using Core.PoolSystem;
using Data;
using Order;
using UI.Components;

namespace UI.Orders
{
    public class OrderCardView : MonoBehaviour , IPoolable
    {
        [Header("UI References")]
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _cardBackground;
        [SerializeField] private Button _completeButton;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Transform _itemsContainer; 
        [SerializeField] private Color _defaultBackgroundColor; 
        [SerializeField] private AnimatedLayoutElement _animatedLayoutElement; 
        public AnimatedLayoutElement AnimatedLayoutElement => _animatedLayoutElement;
        public RectTransform CompleteButtonRect => (RectTransform)_completeButton.transform;
        
        public event Action OnCompleteClicked;
        public event Action<string, int> OnItemDetailRequested;
        
        private ObjectPoolManager _poolManager;

        private readonly Dictionary<ItemIdentifier, OrderItemView> _spawnedItems = new Dictionary<ItemIdentifier, OrderItemView>();

        public TextMeshProUGUI RewardText => _rewardText;

        public void Initialize(ObjectPoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        private void _addRequiredItem(ItemIdentifier identifier, Sprite icon, int requiredCount)
        {
            OrderItemView itemView = _poolManager.SpawnUI<OrderItemView>(PoolObjectType.OrderItem, _itemsContainer);
            itemView.Setup(icon, 0, requiredCount, identifier.Id, identifier.Level);
            itemView.OnOrderItemClicked += _handleItemViewClicked;
            _spawnedItems.Add(identifier, itemView);
        }

        private void Awake()
        {
            _completeButton.onClick.AddListener(() => OnCompleteClicked?.Invoke());
        }

        public void SetupCard(Sprite characterSprite, int rewardAmount, List<OrderItemUIData> requiredItemsData)
        {
            ClearItems();
            _characterImage.sprite = characterSprite;
            _rewardText.text = rewardAmount.ToString();
            
            foreach (var itemData in requiredItemsData)
            {
                _addRequiredItem(itemData.Identifier, itemData.Icon, itemData.RequiredCount);
            }
            
            _cardBackground.color = _defaultBackgroundColor;
            _completeButton.gameObject.SetActive(false);
        }

        private void _handleItemViewClicked(string itemId, int itemLevel)
        {
            OnItemDetailRequested?.Invoke(itemId, itemLevel);
        }
        
        public void HandleOrderProgressUpdate(IReadOnlyDictionary<ItemIdentifier, int> currentCounts)
        {
            foreach (var kvp in currentCounts)
            {
                ItemIdentifier identifier = kvp.Key;
                int currentCount = kvp.Value;

                if (_spawnedItems.TryGetValue(identifier, out OrderItemView itemView))
                {
                    itemView.UpdateCurrentCount(currentCount);
                }
            }
        }
        
        public void SetOrderFullyCompletedState()
        {
            _cardBackground.color = Color.green;
            _completeButton.gameObject.SetActive(true);
        }
        
        public void SetOrderUncompletedState()
        {
            _cardBackground.color = _defaultBackgroundColor;
            _completeButton.gameObject.SetActive(false);
        }

        public void OnSpawned()
        {
            transform.localScale = Vector3.one;
            
            _completeButton.gameObject.SetActive(false);
            _rewardText.text = string.Empty;
        }

        public void OnDespawned()
        {
            _characterImage.sprite = null;
            ClearItems(); 
        }

        private void ClearItems()
        {
            foreach (var item in _spawnedItems.Values)
            {
                item.OnOrderItemClicked -= _handleItemViewClicked;
                _poolManager.Despawn(PoolObjectType.OrderItem, item.gameObject);
            }
            _spawnedItems.Clear();
        }
    }
    
}