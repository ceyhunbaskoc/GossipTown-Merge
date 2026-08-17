using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;

namespace Order
{
    public class OrderModel
    {
        private Dictionary<ItemIdentifier, int> _itemOrders;
        private Dictionary<ItemIdentifier, int> _itemCounts;
        public int RewardAmount { get; }
        
        public IReadOnlyDictionary<ItemIdentifier, int> ItemCounts => _itemCounts;
        public IReadOnlyDictionary<ItemIdentifier, int> ItemOrders => _itemOrders;
        
        public OrderModel(Dictionary<ItemIdentifier, int> itemOrders, int rewardAmount)
        {
            _itemOrders = itemOrders;
            RewardAmount = rewardAmount;
            _itemCounts = new Dictionary<ItemIdentifier, int>();
            foreach (var itemData in itemOrders.Keys)
            {
                _itemCounts.Add(itemData, 0);
            }
        }
        
        public void SyncProgress(ItemIdentifier identifier, int currentBoardCount)
        {
            if (_itemCounts.ContainsKey(identifier))
            {
                _itemCounts[identifier] = Mathf.Min(currentBoardCount, _itemOrders[identifier]);
            }
        }
        
        public bool CheckOrderItemCompletion(ItemData item)
        {
            if (item == null) return false;
            
            var identifier = new ItemIdentifier(item.Id, item.Level);

            if (_itemOrders.TryGetValue(identifier, out int requiredCount) && 
                _itemCounts.TryGetValue(identifier, out int currentCount))
            {
                return currentCount >= requiredCount;
            }
            return false;
        }
        
        public bool IsOrderFullyCompleted()
        {
            foreach (var kvp in _itemOrders)
            {
                if (_itemCounts[kvp.Key] < kvp.Value) return false;
            }
            return true;
        }
    }
}