using System;
using System.Collections.Generic;
using Core.GridSystem;
using Core.SaveSystem;

namespace Order
{
    public class OrderDataModel : IReadOnlyOrder, IOrderModifier
    {
        private List<OrderModel> _currentOrders = new List<OrderModel>();
        
        public event Action<OrderModel> OnOrderCreated;
        public event Action<OrderModel> OnOrderCompleted;
        public event Action<OrderModel> OnOrderUpdated;
        
        public int ActiveOrderCount => _currentOrders.Count;
        public IReadOnlyList<OrderModel> ActiveOrders => _currentOrders;
        
        public bool TryCreateOrder(OrderModel orderModel)
        {
            if (orderModel == null || _currentOrders.Contains(orderModel)) return false;

            _currentOrders.Add(orderModel);
            OnOrderCreated?.Invoke(orderModel);
            return true;
        }

        public void SyncAllOrders(IReadOnlyDictionary<ItemIdentifier, int> boardInventory)
        {
            foreach (var order in _currentOrders)
            {
                foreach (var requiredItem in order.ItemOrders.Keys)
                {
                    int countOnBoard = boardInventory.ContainsKey(requiredItem) ? boardInventory[requiredItem] : 0;
                    order.SyncProgress(requiredItem, countOnBoard);
                }
                OnOrderUpdated?.Invoke(order);
            }
        }
        
        public bool TryCompleteOrder(OrderModel orderModel)
        {
            if(orderModel == null || !_currentOrders.Contains(orderModel)) return false;

            _currentOrders.Remove(orderModel);
            OnOrderCompleted?.Invoke(orderModel);
            return true;
        }

        public List<OrderSaveData> GetSaveData()
        {
            List<OrderSaveData> saveList = new List<OrderSaveData>();

            foreach (var order in _currentOrders)
            {
                OrderSaveData orderData = new OrderSaveData
                {
                    RewardAmount = order.RewardAmount
                };

                foreach (var kvp in order.ItemOrders)
                {
                    orderData.RequiredItems.Add(new OrderItemSaveData
                    {
                        ItemId = kvp.Key.Id,
                        Level = kvp.Key.Level,
                        RequiredCount = kvp.Value
                    });
                }

                saveList.Add(orderData);
            }

            return saveList;
        }

        public void LoadSaveData(List<OrderSaveData> savedDataList)
        {
            if(savedDataList == null || savedDataList.Count == 0) return;
            _currentOrders.Clear();
            foreach(var saveData in savedDataList)
            {
                Dictionary<ItemIdentifier, int> reconstructedItems = new Dictionary<ItemIdentifier, int>();
                foreach (var reqItem in saveData.RequiredItems)
                {
                    reconstructedItems.Add(new ItemIdentifier(reqItem.ItemId, reqItem.Level), reqItem.RequiredCount);
                }
                OrderModel loadedOrder = new OrderModel(reconstructedItems, saveData.RewardAmount);
                TryCreateOrder(loadedOrder);
            }
        }
    }
}