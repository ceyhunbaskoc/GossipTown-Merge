using System;
using Core.Services;
using Data.EventChannels;
using Order;

namespace UI.Orders
{
    public class OrderCardPresenter : IDisposable
    {
        private readonly OrderModel _model;
        private readonly OrderCardView _view;
        private readonly OrderDataModel _orderData;
        private readonly IOrderFulfillmentService _fulfillmentService;
        private readonly ItemDetailEventChannelSO _itemDetailEventChannel;

        public OrderCardView OrderCardView => _view;
        
        public OrderCardPresenter(
            OrderModel model, 
            OrderCardView view, 
            OrderDataModel orderData,
            IOrderFulfillmentService fulfillmentService,
            ItemDetailEventChannelSO itemDetailEventChannel)
        {
            _model = model;
            _view = view;
            _orderData = orderData;
            _fulfillmentService = fulfillmentService;
            _itemDetailEventChannel = itemDetailEventChannel;

            _orderData.OnOrderUpdated += HandleOrderUpdated;
            _view.OnCompleteClicked += HandleCompleteClicked;
            _view.OnItemDetailRequested += _handleItemDetailRequested;
            
            HandleOrderUpdated(_model);
        }
        private void HandleOrderUpdated(OrderModel updatedModel)
        {
            if (updatedModel != _model) return;
            
            _view.HandleOrderProgressUpdate(_model.ItemCounts);
            
            if (_model.IsOrderFullyCompleted())
            {
                _view.SetOrderFullyCompletedState();
            }
            else
            {
                _view.SetOrderUncompletedState();
            }
        }

        private void _handleItemDetailRequested(string itemId, int itemLevel)
        {
            _itemDetailEventChannel.RaiseEvent(new ItemDetailRequest{ItemId = itemId, ItemLevel = itemLevel});
        }

        private void HandleCompleteClicked()
        {
            _fulfillmentService.ProcessFulfillment(_model, _view.transform, _view.AnimatedLayoutElement);
        }

        public void Dispose()
        {
            _orderData.OnOrderUpdated -= HandleOrderUpdated;
            _view.OnCompleteClicked -= HandleCompleteClicked;
        }
    }
}