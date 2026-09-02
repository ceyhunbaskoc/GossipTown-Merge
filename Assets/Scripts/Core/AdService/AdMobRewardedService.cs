using System;
using UnityEngine;
using GoogleMobileAds.Api;
using Core.Architecture;

namespace Core.AdService
{
    public class AdMobRewardedService : IAdService, IDisposable
    {
        private RewardedAd _rewardedAd;
        private readonly string _adUnitId;

        private Action _onAdWatched;
        private Action _onAdFailed;
        private bool _isRewardEarned;
        
        public AdMobRewardedService(string adUnitId)
        {
            _adUnitId = adUnitId;
            InitializeSdk();
        }

        private void InitializeSdk()
        {
            MobileAds.Initialize(initStatus => MainThreadDispatcher.Enqueue(LoadAd));
        }

        private void LoadAd()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            var adRequest = new AdRequest();
            
            RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"[AdMobRewardedService] Ad can't be loaded. Error: {error}");
                        return;
                    }

                    _rewardedAd = ad;
                    RegisterEventHandlers(_rewardedAd);
                });
            });
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += HandleAdClosed;
            ad.OnAdFullScreenContentFailed += HandleAdFailed;
        }

        public void ShowRewardedAd(Action onAdWatched, Action onAdFailed)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _onAdWatched = onAdWatched;
                _onAdFailed = onAdFailed;
                _isRewardEarned = false;

                _rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        _isRewardEarned = true;
                        _onAdWatched?.Invoke();
                    });
                });
            }
            else
            {
                Debug.LogWarning("[AdMobRewardedService] Ad isn't ready yet!");
                onAdFailed?.Invoke();
                
                LoadAd(); 
            }
        }

        private void HandleAdClosed()
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                if (!_isRewardEarned)
                {
                    _onAdFailed?.Invoke();
                }

                ClearCallbacks();
                LoadAd(); 
            });
        }

        private void HandleAdFailed(AdError error)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.LogError($"[AdMobRewardedService] Ad display failed: {error.GetMessage()}");
                _onAdFailed?.Invoke();
                
                ClearCallbacks();
                LoadAd();
            });
        }

        private void ClearCallbacks()
        {
            _onAdWatched = null;
            _onAdFailed = null;
            _isRewardEarned = false;
        }

        public void Dispose()
        {
            ClearCallbacks();
            
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }
    }
}