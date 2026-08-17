using UnityEngine;
using Data; // CurrencyType'ın bulunduğu namespace

namespace Core.Economy.Exchange
{
    public enum ExchangeDirection
    {
        Forward,
        Reverse
    }

    [CreateAssetMenu(fileName = "New Exchange Pair", menuName = "Economy/Exchange Pair")]
    public class ExchangePairSO : ScriptableObject
    {
        [Header("Currencies")]
        public CurrencyType CurrencyA = CurrencyType.Gold;
        public CurrencyType CurrencyB = CurrencyType.Energy;

        [Header("Forward Exchange (A -> B)")]
        public int ForwardCostAmount = 20;
        public int ForwardRewardAmount = 10;

        [Header("Reverse Exchange (B -> A)")]
        public int ReverseCostAmount = 20;
        public int ReverseRewardAmount = 15;
    }
}