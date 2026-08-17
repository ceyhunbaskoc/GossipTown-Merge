using System.Collections.Generic;
using Core.Economy.Shop;
using Core.PoolSystem;
using Data.Economy.Shop;
using UI.Shop;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Economy
{
    public class ShopAdUIPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _goldContainer;
        [SerializeField] private Transform _gemContainer;
        [SerializeField] private GameObject _panelView;
        [SerializeField] private Button _panelCloseButton;
        [SerializeField] private Button _panelOpenButton;
        [SerializeField] private Button _panelOpenButtonEnergy;
        [SerializeField] private Button _panelOpenButtonGold;
        [SerializeField] private Button _panelOpenButtonGem;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        private IAdShopService _adShopService;
        private IObjectPool _objectPool;
        
        private readonly Dictionary<string, ShopPurchaseElementView> _spawnedCards = new Dictionary<string, ShopPurchaseElementView>();
        private readonly Dictionary<string, AdShopPackage> _packageDataMap = new Dictionary<string, AdShopPackage>();
        
        private AdShopPackagesDatabaseSO _adPackagesDatabase;

        public void Initialize(IAdShopService adShopService, AdShopPackagesDatabaseSO adShopPackagesDatabase, IObjectPool objectPool)
        {
            _adShopService = adShopService;
            _adPackagesDatabase = adShopPackagesDatabase;
            _objectPool = objectPool;
            
            SpawnAndSetupCards();
            SubscribeToEvents();
        }

        private void SpawnAndSetupCards()
        {
            foreach (var package in _adPackagesDatabase.AdShopPackages)
            {
                Transform targetContainer = package.RewardCurrency == Data.CurrencyType.Gold ? _goldContainer : _gemContainer;

                ShopPurchaseElementView cardView =
                    _objectPool.SpawnUI<ShopPurchaseElementView>(PoolObjectType.ShopPurchaseItem, targetContainer);
                
                int watchedCount = _adShopService.GetWatchedCount(package.PackageId);
                
                cardView.Setup(package.Title, package.Icon, watchedCount, package.DailyAdLimit, package.RewardAmount);
                
                AdShopPackage currentPackage = package;
                cardView.OnPurchaseButtonClicked += () => _adShopService.ProcessAdPurchase(currentPackage);

                _spawnedCards.Add(package.PackageId, cardView);
                _packageDataMap.Add(package.PackageId, package);
            }
        }

        private void SubscribeToEvents()
        {
            _adShopService.OnAdFlowStarted += LockAllCards;
            _adShopService.OnAdFlowEnded += UnlockAvailableCards;
            _adShopService.OnPackageUpdated += UpdateSpecificCard;
            
            _panelOpenButton.onClick.AddListener(_openPanel);
            _panelOpenButtonEnergy.onClick.AddListener(_openPanel);
            _panelOpenButtonGold.onClick.AddListener(_openPanel);
            _panelOpenButtonGem.onClick.AddListener(_openPanel);
            _panelCloseButton.onClick.AddListener(_closePanel);
        }

        private void LockAllCards()
        {
            foreach (var card in _spawnedCards.Values)
            {
                card.HandleAnyAdRequested();
            }
        }

        private void UnlockAvailableCards()
        {
            foreach (var kvp in _spawnedCards)
            {
                string packageId = kvp.Key;
                ShopPurchaseElementView card = kvp.Value;
                AdShopPackage packageData = _packageDataMap[packageId];

                int watchedCount = _adShopService.GetWatchedCount(packageId);

                if (watchedCount < packageData.DailyAdLimit)
                {
                    card.HandleAnyAdCompleted();
                }
            }
        }

        private void _openPanel()
        {
            _panelView.SetActive(true);
            _popupAnimator.Show();
        }

        private void _closePanel()
        {
            _popupAnimator.Hide(() =>
            {
                _panelView.SetActive(false);
            });
        }

        private void UpdateSpecificCard(string packageId)
        {
            if (_spawnedCards.TryGetValue(packageId, out ShopPurchaseElementView card))
            {
                AdShopPackage packageData = _packageDataMap[packageId];
                int watchedCount = _adShopService.GetWatchedCount(packageId);
                
                card.Setup(packageData.Title, packageData.Icon, watchedCount, packageData.DailyAdLimit, packageData.RewardAmount);
            }
        }

        private void OnDestroy()
        {
            if (_adShopService != null)
            {
                _adShopService.OnAdFlowStarted -= LockAllCards;
                _adShopService.OnAdFlowEnded -= UnlockAvailableCards;
                _adShopService.OnPackageUpdated -= UpdateSpecificCard;
                
                _spawnedCards.Clear();
                _packageDataMap.Clear();
            }
            
            _panelOpenButton.onClick.RemoveListener(_openPanel);
            _panelOpenButtonEnergy.onClick.RemoveListener(_openPanel);
            _panelOpenButtonGold.onClick.RemoveListener(_openPanel);
            _panelOpenButtonGem.onClick.RemoveListener(_openPanel);
            _panelCloseButton.onClick.RemoveListener(_closePanel);
        }
    }
}