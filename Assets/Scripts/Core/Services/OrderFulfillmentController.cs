using System;
using Core.Audio;
using Core.Controllers;
using Core.Economy;
using Core.Haptics;
using Core.Orchestration;
using Core.PoolSystem;
using Data.Audio;
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
        private readonly IAudioService _audioService;
        private readonly IHapticService _hapticService;

        public OrderFulfillmentController(
            MainBoardController gridController, 
            OrderFulfillmentOrchestrator orchestrator,
            OrderDataModel orderData,
            IEconomyModifier economyModifier,
            ObjectPoolManager poolManager,
            IAudioService audioService,
            IHapticService hapticService)
        {
            _gridController = gridController;
            _orchestrator = orchestrator;
            _orderData = orderData;
            _economyModifier = economyModifier;
            _poolManager = poolManager;
            _audioService = audioService;
            _hapticService = hapticService;
        }

        public event Action OnAnyUserInteraction;

        public void ProcessFulfillment(OrderModel order, Transform targetCardTransform, AnimatedLayoutElement orderCardAnimatedLayoutElement)
        {
            var flyingVisuals = _gridController.ExtractVisualsForOrder(order.ItemOrders);

            _orchestrator.PlayFulfillmentSequence(flyingVisuals, targetCardTransform, () => 
            {
                _economyModifier.AddGold(order.RewardAmount);
                if (_orderData.TryCompleteOrder(order))
                {
                    _audioService.PlaySFX(SfxId.Gameplay_OrderComplete);
                    _hapticService.Play(HapticType.Success);
                }
                OnAnyUserInteraction?.Invoke();
                orderCardAnimatedLayoutElement.Hide(() => 
                {
                    _poolManager.Despawn(PoolObjectType.OrderCard, targetCardTransform.gameObject);
                });
            });
        }
    }
}