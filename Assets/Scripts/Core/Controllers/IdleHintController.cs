using System;
using Core.GridSystem;
using Core.Services;
using UI.Hints;
using UI.Orders;
using UnityEngine;

namespace Core.Controllers
{
    public class IdleHintController : IDisposable
    {
        private readonly IdleMonitorService _idleMonitorService;
        private readonly MergeHintService _hintService;
        private readonly HintVisualOrchestrator _visualOrchestrator;
        private readonly MainBoardController _gridController;
        private readonly IOrderFulfillmentService _orderFulfillmentController;

        public IdleHintController(
            IdleMonitorService idleMonitorService,
            MergeHintService hintService,
            HintVisualOrchestrator visualOrchestrator,
            MainBoardController gridController,
            IOrderFulfillmentService orderFulfillmentController)
        {
            _idleMonitorService = idleMonitorService;
            _hintService = hintService;
            _visualOrchestrator = visualOrchestrator;
            _gridController = gridController;
            _orderFulfillmentController = orderFulfillmentController;

            _idleMonitorService.OnPlayerIdle += HandlePlayerIdle;
            _gridController.OnAnyUserInteraction += HandleUserInteraction;
            _orderFulfillmentController.OnAnyUserInteraction += HandleUserInteraction;
        }

        private void HandlePlayerIdle()
        {
            if (_hintService.TryGetMergeablePair(out Vector2Int pos1, out Vector2Int pos2))
            {
                IViewItem view1 = _gridController.GetVisualAt(pos1);
                IViewItem view2 = _gridController.GetVisualAt(pos2);

                if (view1 != null && view2 != null)
                {
                    _visualOrchestrator.PlayHintAnimation(view1, view2);
                }
            }
        }

        private void HandleUserInteraction()
        {
            _visualOrchestrator.StopHintAnimation();
            _idleMonitorService.ResetTimer();
        }

        public void Dispose()
        {
            _idleMonitorService.OnPlayerIdle -= HandlePlayerIdle;
            _gridController.OnAnyUserInteraction -= HandleUserInteraction;
            _orderFulfillmentController.OnAnyUserInteraction -= HandleUserInteraction;
        }
    }
}