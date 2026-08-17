using System;
using Order;
using UI.Components;
using UnityEngine;

namespace Core.Services
{
    public interface IOrderFulfillmentService
    {
        void ProcessFulfillment(OrderModel order, Transform targetCardTransform, AnimatedLayoutElement orderCardAnimatedLayoutElement);
        event Action OnAnyUserInteraction;
    }
}