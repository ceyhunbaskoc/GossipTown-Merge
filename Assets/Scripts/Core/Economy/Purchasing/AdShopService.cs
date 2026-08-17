using System;
using System.Collections.Generic;
using Core.Economy.Purchasing;
using Core.SaveSystem;
using Data;
using Data.Economy.Shop;
using UnityEngine;

namespace Core.Economy.Shop
{
    public interface IAdShopService
    {
        event Action OnAdFlowStarted;
        event Action OnAdFlowEnded;
        event Action<string> OnPackageUpdated;

        int GetWatchedCount(string packageId);
        void ProcessAdPurchase(AdShopPackage package);
    }

    public class AdShopService : IAdShopService
    {
        public event Action OnAdFlowStarted;
        public event Action OnAdFlowEnded;
        public event Action<string> OnPackageUpdated;

        private readonly IPurchaseStrategy _purchaseStrategy;
        private readonly PlayerEconomyModel _economyModel;
        
        private readonly Dictionary<string, int> _dailyWatchCounts;
        private long _lastResetTimestampTicks;

        public AdShopService(IPurchaseStrategy purchaseStrategy, PlayerEconomyModel economyModel)
        {
            _purchaseStrategy = purchaseStrategy;
            _economyModel = economyModel;
            _dailyWatchCounts = new Dictionary<string, int>();
        }

        public int GetWatchedCount(string packageId)
        {
            return _dailyWatchCounts.TryGetValue(packageId, out int count) ? count : 0;
        }
        
        public void LoadSaveData(AdShopSaveData saveData)
        {
            if (saveData == null) return;

            _dailyWatchCounts.Clear();
            _lastResetTimestampTicks = saveData.LastResetTimestampTicks;

            DateTime lastResetDate = new DateTime(_lastResetTimestampTicks, DateTimeKind.Utc).Date;
            DateTime currentDate = DateTime.UtcNow.Date;

            if (currentDate > lastResetDate)
            {
                _lastResetTimestampTicks = currentDate.Ticks;
            }
            else
            {
                foreach (var watchData in saveData.WatchCounts)
                {
                    _dailyWatchCounts[watchData.PackageId] = watchData.WatchCount;
                }
            }
        }

        public AdShopSaveData GetSaveData()
        {
            AdShopSaveData saveData = new AdShopSaveData
            {
                LastResetTimestampTicks = _lastResetTimestampTicks
            };

            foreach (var kvp in _dailyWatchCounts)
            {
                saveData.WatchCounts.Add(new AdPackageWatchData
                {
                    PackageId = kvp.Key,
                    WatchCount = kvp.Value
                });
            }

            return saveData;
        }

        public void ProcessAdPurchase(AdShopPackage package)
        {
            int currentWatched = GetWatchedCount(package.PackageId);
            
            if (currentWatched >= package.DailyAdLimit) return;

            OnAdFlowStarted?.Invoke();

            _purchaseStrategy.ProcessPurchase(
                package.DailyAdLimit,
                onSuccess: () => 
                {
                    GrantReward(package.RewardCurrency, package.RewardAmount);
                    
                    _dailyWatchCounts[package.PackageId] = currentWatched + 1;
                    
                    if (_lastResetTimestampTicks == 0) 
                    {
                        _lastResetTimestampTicks = DateTime.UtcNow.Date.Ticks;
                    }
                    
                    OnPackageUpdated?.Invoke(package.PackageId);
                    OnAdFlowEnded?.Invoke();
                },
                onFailure: () => 
                {
                    OnAdFlowEnded?.Invoke();
                }
            );
        }

        private void GrantReward(CurrencyType currency, int amount)
        {
            if (currency == CurrencyType.Gold) _economyModel.AddGold(amount);
            else if (currency == CurrencyType.Energy) _economyModel.AddEnergy(amount);
        }
    }
}