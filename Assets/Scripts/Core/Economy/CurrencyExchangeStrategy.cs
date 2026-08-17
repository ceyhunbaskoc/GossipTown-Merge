using System;
using Core.Economy.Exchange;
using Data;

namespace Core.Economy
{
    public interface ICurrencyExchangeService
    {
        void TryExchange(ExchangePairSO pair, ExchangeDirection direction, Action onSuccess, Action onFailure);
    }

    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly PlayerEconomyModel _economyModel;

        public CurrencyExchangeService(PlayerEconomyModel economyModel)
        {
            _economyModel = economyModel;
        }

        public void TryExchange(ExchangePairSO pair, ExchangeDirection direction, Action onSuccess, Action onFailure)
        {
            if (pair == null)
            {
                onFailure?.Invoke();
                return;
            }
            CurrencyType costCurrency = direction == ExchangeDirection.Forward ? pair.CurrencyA : pair.CurrencyB;
            int costAmount = direction == ExchangeDirection.Forward ? pair.ForwardCostAmount : pair.ReverseCostAmount;

            CurrencyType rewardCurrency = direction == ExchangeDirection.Forward ? pair.CurrencyB : pair.CurrencyA;
            int rewardAmount = direction == ExchangeDirection.Forward ? pair.ForwardRewardAmount : pair.ReverseRewardAmount;

            if (!HasEnoughCurrency(costCurrency, costAmount))
            {
                onFailure?.Invoke();
                return;
            }

            SpendCurrency(costCurrency, costAmount);
            GrantCurrency(rewardCurrency, rewardAmount);

            onSuccess?.Invoke();
        }

        private bool HasEnoughCurrency(CurrencyType type, int amount)
        {
            return type switch
            {
                CurrencyType.Gem => _economyModel.Gems >= amount,
                CurrencyType.Energy => _economyModel.Energy >= amount,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private void SpendCurrency(CurrencyType type, int amount)
        {
            switch (type)
            {
                case CurrencyType.Gem: _economyModel.TrySpendGems(amount); break;
                case CurrencyType.Energy: _economyModel.TrySpendEnergy(amount); break;
            }
        }

        private void GrantCurrency(CurrencyType type, int amount)
        {
            switch (type)
            {
                case CurrencyType.Gem: _economyModel.AddGem(amount); break;
                case CurrencyType.Energy: _economyModel.AddEnergy(amount); break;
            }
        }
    }
}