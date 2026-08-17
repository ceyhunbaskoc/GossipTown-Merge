using System.Collections.Generic;
using Core.Discovery;
using Core.GridSystem;
using Core.Services;
using Data;
using UnityEngine;

namespace Order
{
    public class OrderGenerationService
    {
        private readonly IReadOnlyItemDiscovery _itemDiscovery;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly IOrderModifier _orderModifier;
        private readonly IBoardInventoryProvider _boardInventory;
        private readonly IOrderSelectionStrategy _selectionStrategy;
        private readonly IOrderPricingStrategy _pricingStrategy;

        public OrderGenerationService(
            IReadOnlyItemDiscovery itemDiscovery, 
            ItemDatabaseSO itemDatabase, 
            IOrderModifier orderModifier,
            IBoardInventoryProvider boardInventory,
            IOrderSelectionStrategy selectionStrategy,
            IOrderPricingStrategy pricingStrategy)
        {
            _itemDiscovery = itemDiscovery;
            _itemDatabase = itemDatabase;
            _orderModifier = orderModifier;
            _boardInventory = boardInventory;
            _selectionStrategy = selectionStrategy;
            _pricingStrategy = pricingStrategy;
        }

        public bool TryGenerateOrderData(int itemCount)
        {
            List<ItemDefinitionSO> availableItems = GetAvailableItems();
            if (availableItems.Count == 0) return false;
            
            Dictionary<ItemIdentifier, int> newOrderItems = new Dictionary<ItemIdentifier, int>();
            int totalOrderValue = 0;
            
            for(int i = 0; i < itemCount; i++)
            {
                ItemIdentifier selectedItem = _selectionStrategy.SelectItem(availableItems, _itemDiscovery, _boardInventory);
                
                if (newOrderItems.ContainsKey(selectedItem)) newOrderItems[selectedItem]++;
                else newOrderItems[selectedItem] = 1;
                
                totalOrderValue += _pricingStrategy.CalculateReward(3, selectedItem.Level);
            }
            
            int finalReward = Mathf.RoundToInt(totalOrderValue * UnityEngine.Random.Range(0.9f, 1.1f));
            OrderModel newOrder = new OrderModel(newOrderItems, finalReward);
            
            _orderModifier.TryCreateOrder(newOrder);
            return true;
        }
        
        private List<ItemDefinitionSO> GetAvailableItems()
        {
            var availableItems = new List<ItemDefinitionSO>();
            foreach (var item in _itemDatabase.NormalItems)
            {
                if (_itemDiscovery.IsItemUnlocked(item.Id, 1)) availableItems.Add(item);
            }
            return availableItems;
        }
    }
}