using System;
using Core.Reward;
using Core.SaveSystem;
using Core.Services;

namespace Order
{
    public class OrderMilestoneTracker : IDisposable
    {
        private readonly OrderDataModel _orderData;
        private readonly RewardDispatcherService _rewardDispatcher;
        private readonly IRewardSelectionService _rewardSelectionService;
        
        private int _completedOrdersInSeries;
        private readonly int _seriesGoal;

        public OrderMilestoneTracker(
            OrderDataModel orderData, 
            RewardDispatcherService rewardDispatcher,
            IRewardSelectionService rewardSelectionService,
            int seriesGoal = 2)
        {
            _orderData = orderData;
            _rewardDispatcher = rewardDispatcher;
            _rewardSelectionService = rewardSelectionService;
            _seriesGoal = seriesGoal;

            _orderData.OnOrderCompleted += HandleOrderCompleted;
        }

        private void HandleOrderCompleted(OrderModel completedOrder)
        {
            _completedOrdersInSeries++;

            if (_completedOrdersInSeries >= _seriesGoal)
            {
                RewardPayload calculatedReward = _rewardSelectionService.DetermineReward();
                
                if (calculatedReward.IsValid) 
                {
                    _rewardDispatcher.DispatchReward(calculatedReward);
                }

                _completedOrdersInSeries = 0;
            }
        }

        public OrderMilestoneSaveData GetSaveData()
        {
            return new OrderMilestoneSaveData
            {
                CompletedOrdersInSeries = _completedOrdersInSeries
            };
        }

        public void LoadSaveData(OrderMilestoneSaveData saveData)
        {
            _completedOrdersInSeries = saveData.CompletedOrdersInSeries;
        }

        public void Dispose()
        {
            _orderData.OnOrderCompleted -= HandleOrderCompleted;
        }
    }
}