using System;
using System.Collections.Generic;
using Core.LevelSystem;
using UnityEngine;

namespace Order
{
    public class OrderWaveController : IDisposable
    {
        private readonly OrderDataModel _orderData;
        private readonly OrderGenerationService _orderGenerator;
        
        private readonly IWaveRewardCalculator _waveRewardCalculator;
        private readonly ILevelModifier _levelModifier;
        
        private readonly int _minOrdersPerWave;
        private readonly int _baseMaxOrdersPerWave;
        private readonly int _wavesPerIncrement;
        private readonly int _absoluteMaxOrdersLimit;
        
        private int _currentWaveCount = 1;
        
        private readonly List<OrderModel> _completedOrdersInCurrentWave = new List<OrderModel>();

        public OrderWaveController(
            OrderDataModel orderData, 
            OrderGenerationService orderGenerator,
            IWaveRewardCalculator waveRewardCalculator,
            ILevelModifier levelModifier,
            int minOrdersPerWave = 2,
            int baseMaxOrdersPerWave = 4,
            int wavesPerIncrement  = 2,
            int absoluteMaxOrdersLimit = 6)
        {
            _orderData = orderData;
            _orderGenerator = orderGenerator;
            _minOrdersPerWave = minOrdersPerWave;
            _baseMaxOrdersPerWave = baseMaxOrdersPerWave;
            _wavesPerIncrement = wavesPerIncrement;
            _absoluteMaxOrdersLimit = absoluteMaxOrdersLimit;
            _waveRewardCalculator = waveRewardCalculator;
            _levelModifier = levelModifier;

            _orderData.OnOrderCompleted += HandleOrderCompleted;
        }

        private void HandleOrderCompleted(OrderModel completedOrder)
        {
            _completedOrdersInCurrentWave.Add(completedOrder);
            if (_orderData.ActiveOrderCount <= 0)
            {
                ProcessWaveCompletion();
                GenerateNewWave();
            }
        }
        
        private void ProcessWaveCompletion()
        {
            int waveExperience = _waveRewardCalculator.CalculateWaveExperience(_completedOrdersInCurrentWave);
            _levelModifier.TryAddExperience(waveExperience);
            _completedOrdersInCurrentWave.Clear();
        }

        public void GenerateNewWave()
        {
            int calculatedMax = _baseMaxOrdersPerWave + (_currentWaveCount / _wavesPerIncrement);
            
            int currentMaxOrders = Mathf.Min(calculatedMax, _absoluteMaxOrdersLimit);
            
            int targetOrderCount = UnityEngine.Random.Range(_minOrdersPerWave, currentMaxOrders + 1);
            
            for (int i = 0; i < targetOrderCount; i++)
            {
                int randomItemVariety = UnityEngine.Random.Range(1, 4);
                _orderGenerator.TryGenerateOrderData(randomItemVariety);
            }

            _currentWaveCount++;
        }

        public void SetWaveCountFromSave(int loadedWaveCount)
        {
            _currentWaveCount = loadedWaveCount;
        }

        public void Dispose()
        {
            _orderData.OnOrderCompleted -= HandleOrderCompleted;
        }
    }
}