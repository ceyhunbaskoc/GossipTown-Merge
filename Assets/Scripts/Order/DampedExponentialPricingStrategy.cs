using UnityEngine;

namespace Order
{
    public interface IOrderPricingStrategy
    {
        int CalculateReward(int baseValue, int itemLevel);
    }

    public class DampedExponentialPricingStrategy : IOrderPricingStrategy
    {
        private readonly float _dampingFactor;

        public DampedExponentialPricingStrategy(float dampingFactor = 0.8f)
        {
            _dampingFactor = dampingFactor;
        }

        public int CalculateReward(int baseValue, int itemLevel)
        {
            if (itemLevel < 1) return baseValue;

            int trueEffort = 1 << (itemLevel - 1);
            
            float dampedEffort = Mathf.Pow(trueEffort, _dampingFactor);
            
            return Mathf.RoundToInt(baseValue * dampedEffort);
        }
    }
}