using System;
using Core.Controllers;
using Core.Economy;
using Core.Orchestration;
using Core.PoolSystem;
using Order;
using UI.Components;
using UnityEngine;

namespace Core.Services
{

    public class OrderFulfillmentController : IOrderFulfillmentService
    {
        private readonly MainBoardController _gridController;
        private readonly OrderFulfillmentOrchestrator _orchestrator;
        private readonly OrderDataModel _orderData;
        private readonly IEconomyModifier _economyModifier;
        private readonly ObjectPoolManager _poolManager;

        public OrderFulfillmentController(
            MainBoardController gridController, 
            OrderFulfillmentOrchestrator orchestrator,
            OrderDataModel orderData,
            IEconomyModifier economyModifier,
            ObjectPoolManager poolManager)
        {
            _gridController = gridController;
            _orchestrator = orchestrator;
            _orderData = orderData;
            _economyModifier = economyModifier;
            _poolManager = poolManager;
        }

        public event Action OnAnyUserInteraction;

        public void ProcessFulfillment(OrderModel order, Transform targetCardTransform, AnimatedLayoutElement orderCardAnimatedLayoutElement)
        {
            var flyingVisuals = _gridController.ExtractVisualsForOrder(order.ItemOrders);

            _orchestrator.PlayFulfillmentSequence(flyingVisuals, targetCardTransform, () => 
            {
                _economyModifier.AddGold(order.RewardAmount);
                _orderData.TryCompleteOrder(order);
                OnAnyUserInteraction?.Invoke();
                orderCardAnimatedLayoutElement.Hide(() => 
                {
                    _poolManager.Despawn(PoolObjectType.OrderCard, targetCardTransform.gameObject);
                });
            });
        }
    }
}