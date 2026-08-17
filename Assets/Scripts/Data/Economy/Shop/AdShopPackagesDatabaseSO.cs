using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Economy.Shop
{
    [Serializable]
    public class AdShopPackage
    {
        [Header("Identity")]
        public string PackageId;
        public string Title;
        public Sprite Icon;

        [Header("Reward")]
        public CurrencyType RewardCurrency;
        public int RewardAmount;

        [Header("Limits")]
        public int DailyAdLimit;
    }
    
    [CreateAssetMenu(fileName = "New Ad Shop Packages Database", menuName = "Economy/Ad Shop Packages Database")]
    public class AdShopPackagesDatabaseSO : ScriptableObject
    {
        [field: SerializeField] public List<AdShopPackage> AdShopPackages { get; private set; }
    }
}