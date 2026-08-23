using System.Collections.Generic;
using Core.GridSystem;
using Core.PoolSystem;
using Core.Services;
using Data;
using Data.EventChannels;
using Data.Quests;
using Data.Reward;
using Order;
using UI.FlightSystem;
using UnityEngine;

namespace UI.Orders
{
    public class OrderUIManager : MonoBehaviour
    {
        [SerializeField] private Transform _cardsContainer;
        
        private OrderDataModel _orderDataModel;
        private IOrderFulfillmentService _fulfillmentService;
        private ObjectPoolManager _poolManager;
        private ItemDatabaseSO _itemDatabase;
        private CurrencyFlightService _currencyFlightService;
        
        private IOrderCharacterSelector _characterSelector;
        private ItemDetailEventChannelSO _itemDetailEventChannel;

        private Dictionary<OrderModel, OrderCardPresenter> _activePresenters = new Dictionary<OrderModel, OrderCardPresenter>();
        
        private Dictionary<OrderModel, Sprite> _activeOrderSprites = new Dictionary<OrderModel, Sprite>();

        public void Initialize(OrderDataModel orderDataModel, 
            IOrderFulfillmentService fulfillmentService, 
            ObjectPoolManager poolManager, 
            ItemDatabaseSO itemDatabase,
            CurrencyFlightService currencyFlightService,
            IOrderCharacterSelector characterSelector,
            ItemDetailEventChannelSO itemDetailEventChannel)
        {
            _orderDataModel = orderDataModel;
            _fulfillmentService = fulfillmentService;
            _poolManager = poolManager;
            _itemDatabase = itemDatabase;
            _currencyFlightService = currencyFlightService;
            _characterSelector = characterSelector;
            _itemDetailEventChannel = itemDetailEventChannel;

            _orderDataModel.OnOrderCreated += HandleOrderCreated;
            _orderDataModel.OnOrderCompleted += HandleOrderCompleted;
        }

        private void HandleOrderCreated(OrderModel newOrder)
        {
            GameObject cardObj = _poolManager.Spawn(PoolObjectType.OrderCard, Vector3.zero, Quaternion.identity);
            cardObj.transform.SetParent(_cardsContainer, false);
    
            OrderCardView viewInstance = cardObj.GetComponent<OrderCardView>();
            viewInstance.Initialize(_poolManager);
            viewInstance.AnimatedLayoutElement.Show();

            List<OrderItemUIData> requiredItemsData = new List<OrderItemUIData>();
            
            foreach (var kvp in newOrder.ItemOrders)
            {
                ItemIdentifier identifier = kvp.Key;
                int requiredCount = kvp.Value;

                BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(identifier.Id);
                Sprite itemSprite = itemDef.GetIcon(identifier.Level);

                requiredItemsData.Add(new OrderItemUIData(identifier, itemSprite, requiredCount));
            }
            
            Sprite characterSprite = _characterSelector.GetUniqueCharacterSprite();
            _activeOrderSprites.Add(newOrder, characterSprite);
            
            viewInstance.SetupCard(characterSprite, newOrder.RewardAmount, requiredItemsData);

            OrderCardPresenter presenter = new OrderCardPresenter(newOrder, viewInstance, _orderDataModel, _fulfillmentService, _itemDetailEventChannel);
            _activePresenters.Add(newOrder, presenter);
        }
        
        private void HandleOrderCompleted(OrderModel completedOrder)
        {
            if (_activePresenters.TryGetValue(completedOrder, out OrderCardPresenter clickedView))
            {
                _currencyFlightService.PlayFlightAnimation(
                    RewardCategory.Gold, 
                    completedOrder.RewardAmount, 
                    clickedView.OrderCardView.RewardText.transform.position); 
            }
            
            if (_activePresenters.TryGetValue(completedOrder, out OrderCardPresenter presenter))
            {
                presenter.Dispose(); 
                _activePresenters.Remove(completedOrder);
            }
            
            if (_activeOrderSprites.TryGetValue(completedOrder, out Sprite usedSprite))
            {
                _characterSelector.ReleaseCharacterSprite(usedSprite);
                _activeOrderSprites.Remove(completedOrder);
            }
        }

        private void OnDestroy()
        {
            if (_orderDataModel != null)
            {
                _orderDataModel.OnOrderCreated -= HandleOrderCreated;
                _orderDataModel.OnOrderCompleted -= HandleOrderCompleted;
            }
            
            foreach (var p in _activePresenters.Values) 
            {
                p.Dispose();
            }
            
            foreach (var sprite in _activeOrderSprites.Values)
            {
                _characterSelector.ReleaseCharacterSprite(sprite);
            }
            _activeOrderSprites.Clear();
        }
    }
}