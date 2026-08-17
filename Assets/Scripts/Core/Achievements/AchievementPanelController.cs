using System.Collections.Generic;
using Core.Discovery;
using Core.Economy;
using Core.Services;
using Data;
using Data.Quests;
using Data.Reward;
using UI.Achievements;
using UI.FlightSystem;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Core.Achievements
{
    public class AchievementPanelController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _categoryContainer;
        [SerializeField] private AchievementCategoryView _categoryPrefab;
        [SerializeField] private GameObject _panelUI;
        [SerializeField] private Button _panelButton;
        [SerializeField] private Button _panelCloseButton;
        [SerializeField] private UIPopupAnimator _popupAnimator;
        

        private ItemDatabaseSO _itemDatabase;
        private ItemDiscoveryModel _discoveryModel;
        private AchievementRewardModel _rewardModel;
        private IEconomyModifier _economyModel;
        private IInputLockService _inputLockService;
        private CurrencyFlightService _currencyFlightService;

        private bool _isInitialized = false;

        private readonly Dictionary<(string itemId, int level), AchievementSlotView> _slotRegistry = 
            new Dictionary<(string itemId, int level), AchievementSlotView>();

        private bool _isPanelCurrentlyOpen = false;

        private const int GEM_REWARD_PER_ACHIEVEMENT = 1;

        public void Initialize(
            ItemDatabaseSO itemDatabase, 
            ItemDiscoveryModel discoveryModel, 
            AchievementRewardModel rewardModel,
            IEconomyModifier economyModel,
            IInputLockService inputLockService,
            CurrencyFlightService currencyFlightService)
        {
            _itemDatabase = itemDatabase;
            _discoveryModel = discoveryModel;
            _rewardModel = rewardModel;
            _economyModel = economyModel;
            _inputLockService = inputLockService;
            _currencyFlightService = currencyFlightService;
            
            _panelButton.onClick.AddListener(_openPanel);
            _panelCloseButton.onClick.AddListener(_closePanel);
        }

        private void _openPanel()
        {
            _panelUI.SetActive(true);
            _popupAnimator.Show();

            if (!_isPanelCurrentlyOpen)
            {
                _inputLockService.AddLock();
                _isPanelCurrentlyOpen = true;
            }
            
            if (!_isInitialized)
            {
                _generateAchievementUI();
                _isInitialized = true;
            }
            else
            {
                _refreshAllStates();
            }
        }

        private void _closePanel()
        {
            _popupAnimator.Hide(() =>
            {
                _panelUI.SetActive(false);
            });
            
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
                _isPanelCurrentlyOpen = false;
            }
        }

        private void _generateAchievementUI()
        {
            var allItems = _itemDatabase.GetAllItems(); 

            foreach (var itemDef in allItems)
            {
                AchievementCategoryView categoryView = Instantiate(_categoryPrefab, _categoryContainer);
                categoryView.SetTitle(itemDef.ItemName);

                for (int level = 1; level <= itemDef.MaxLevel; level++)
                {
                    AchievementSlotView slotView = categoryView.CreateSlot();
                    
                    string currentId = itemDef.Id;
                    int currentLevel = level;
                    Sprite icon = itemDef.GetIcon(level);
                    _slotRegistry.Add((currentId, currentLevel), slotView);

                    _bindSlot(slotView, currentId, currentLevel, icon);
                }
            }
        }

        private void _bindSlot(AchievementSlotView slotView, string itemId, int level, Sprite icon)
        {
            AchievementState currentState = DetermineState(itemId, level);
            int rewardAmount = GEM_REWARD_PER_ACHIEVEMENT; 

            AchievementSlotViewModel payload = new AchievementSlotViewModel
            {
                ItemId = itemId,
                Level = level,
                Icon = icon,
                State = currentState,
                RewardAmount = rewardAmount
            };

            slotView.Bind(payload, () => OnClaimRequested(slotView, payload));
        }

        private void _refreshAllStates()
        {
            foreach (var kvp in _slotRegistry)
            {
                (string itemId, int level) = kvp.Key;
                AchievementSlotView slotView = kvp.Value;
                Sprite icon = _itemDatabase.GetItemDef(itemId).GetIcon(level);
                
                _bindSlot(slotView, itemId, level, icon);
            }
        }

        private AchievementState DetermineState(string itemId, int level)
        {
            if (!_discoveryModel.IsItemUnlocked(itemId, level)) return AchievementState.Locked;
            if (_rewardModel.IsRewardClaimed(itemId, level)) return AchievementState.Claimed;
            
            return AchievementState.UnlockedUnclaimed;
        }

        private void OnClaimRequested(AchievementSlotView slotView, AchievementSlotViewModel payload)
        {
            if (DetermineState(payload.ItemId, payload.Level) != AchievementState.UnlockedUnclaimed) return;

            _economyModel.AddGem(payload.RewardAmount); 
            _rewardModel.MarkRewardClaimed(payload.ItemId, payload.Level);
            _currencyFlightService.PlayFlightAnimation(RewardCategory.Gem, payload.RewardAmount, slotView.transform.position);

            payload.State = AchievementState.Claimed;
            slotView.Bind(payload, null); 
        }

        private void OnDestroy()
        {
            _panelButton.onClick.RemoveListener(_openPanel);
            _panelCloseButton.onClick.RemoveListener(_closePanel);
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
            }
        }
    }
}