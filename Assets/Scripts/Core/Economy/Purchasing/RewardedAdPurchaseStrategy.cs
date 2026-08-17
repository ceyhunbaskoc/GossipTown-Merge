using System;
using Core.AdService;

namespace Core.Economy.Purchasing
{
    public class RewardedAdPurchaseStrategy : IPurchaseStrategy
    {
        private readonly IAdService _adService;

        public RewardedAdPurchaseStrategy(IAdService adService)
        {
            _adService = adService;
        }

        public void ProcessPurchase(int requiredAdCount, Action onSuccess, Action onFailure)
        {
            _adService.ShowRewardedAd(
                onAdWatched: () => onSuccess?.Invoke(),
                onAdFailed: () => onFailure?.Invoke()
            );
        }
    }
}