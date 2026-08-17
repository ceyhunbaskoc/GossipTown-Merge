using System;

namespace Core.AdService
{
    public interface IAdService
    {
        void ShowRewardedAd(Action onAdWatched, Action onAdFailed);
    }
}