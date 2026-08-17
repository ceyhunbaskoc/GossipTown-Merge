using System.Collections.Generic;
using Core.GridSystem;

namespace Order
{
    public interface IOrderModifier
    {
        bool TryCreateOrder(OrderModel orderModel);
        void SyncAllOrders(IReadOnlyDictionary<ItemIdentifier, int> boardInventory);
        bool TryCompleteOrder(OrderModel orderModel);
    }
}