using System;

namespace Order
{
    public interface IReadOnlyOrder
    {
        event Action<OrderModel> OnOrderCreated;
        event Action<OrderModel> OnOrderCompleted;
        event Action<OrderModel> OnOrderUpdated;
    }
}