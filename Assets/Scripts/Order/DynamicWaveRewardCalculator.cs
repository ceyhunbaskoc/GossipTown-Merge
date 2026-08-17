using System.Collections.Generic;
using UnityEngine;

namespace Order
{
    public interface IWaveRewardCalculator
    {
        int CalculateWaveExperience(IReadOnlyList<OrderModel> completedOrders);
    }

    public class DynamicWaveRewardCalculator : IWaveRewardCalculator
    {
        private const int BASE_EXP_PER_ORDER = 10;
        private const float DIFFICULTY_MULTIPLIER = 1.5f;
        private const float WAVE_COMPLETION_BONUS = 1.2f;

        public int CalculateWaveExperience(IReadOnlyList<OrderModel> completedOrders)
        {
            if (completedOrders == null || completedOrders.Count == 0)
            {
                return 0;
            }

            int totalWaveExperience = 0;

            foreach (var order in completedOrders)
            {
                totalWaveExperience += CalculateSingleOrderExperience(order);
            }
            float finalExperience = totalWaveExperience * WAVE_COMPLETION_BONUS;

            return Mathf.RoundToInt(finalExperience);
        }

        private int CalculateSingleOrderExperience(OrderModel order)
        {
            int orderExperience = 0;
            
            foreach (var kvp in order.ItemOrders)
            {
                int itemLevel = kvp.Key.Level;
                int requiredCount = kvp.Value;
                
                float rawItemExp = Mathf.Pow(DIFFICULTY_MULTIPLIER, itemLevel) * BASE_EXP_PER_ORDER;
                orderExperience += Mathf.RoundToInt(rawItemExp) * requiredCount;
            }

            return orderExperience;
        }
    }
}