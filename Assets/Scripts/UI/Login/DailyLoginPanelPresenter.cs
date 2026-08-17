using System.Collections.Generic;
using Core.Login;
using Core.Services;
using Data.Login;
using Data.Quests;
using Data.Reward;
using UI.FlightSystem;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Login
{
    public class DailyLoginPanelPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private DailyLoginItemView _itemViewPrefab;
        [SerializeField] private DailyLoginItemView _grandItem;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _closeButton;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        private DailyLoginService _loginService;
        private DailyLoginConfigSO _config;
        private IInputLockService _inputLockService;
        private CurrencyFlightService _currencyFlightService;

        private readonly List<DailyLoginItemView> _cachedViews = new List<DailyLoginItemView>();
        
        private bool _isPanelCurrentlyOpen;
        private bool _isInitialized;

        public void Initialize(
            DailyLoginService loginService, 
            DailyLoginConfigSO config, 
            IInputLockService inputLockService, 
            CurrencyFlightService currencyFlightService) 
        {
            _loginService = loginService;
            _config = config;
            _inputLockService = inputLockService;
            _currencyFlightService = currencyFlightService;

            _closeButton.onClick.AddListener(ClosePanel);
            
            _loginService.OnRewardClaimed += HandleRewardClaimed;
            _loginService.OnNewDayAvailable += HandleNewDayAvailable;

            _panelRoot.SetActive(false);
        }

        public void OpenPanel()
        {
            if (!_isPanelCurrentlyOpen)
            {
                _inputLockService.AddLock();
                _isPanelCurrentlyOpen = true;
            }

            if (!_isInitialized)
            {
                GenerateViews();
                _isInitialized = true;
            }
            RefreshPanelState();
            
            _panelRoot.SetActive(true);
            _popupAnimator.Show();
        }

        private void ClosePanel()
        {
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
                _isPanelCurrentlyOpen = false;
            }
            
            _popupAnimator.Hide(() =>
            {
                _panelRoot.SetActive(false);
            });
            
            if (_loginService != null)
            {
                if (_loginService.IsRewardAvailableToday)
                {
                    _loginService.ClaimTodayReward();
                }
                
                _loginService.AutoClaimPendingMilestones();
            }
        }

        private void GenerateViews()
        {
            for (int i = 0; i < _config.UILoopDays; i++)
            {
                DailyLoginItemView viewInstance;
                
                if (i != _config.UILoopDays - 1)
                {
                    viewInstance = Instantiate(_itemViewPrefab, _itemsContainer);
                }
                else
                {
                    viewInstance = _grandItem;
                }

                viewInstance.gameObject.SetActive(true);
                viewInstance.transform.localScale = Vector3.one;

                viewInstance.OnClaimClicked += ProcessClaimRequest;
                
                _cachedViews.Add(viewInstance);
            }
        }

        private void RefreshPanelState()
        {
            int totalClaimedDays = _loginService.TotalClaimedDays;
            bool isRewardAvailableToday = _loginService.IsRewardAvailableToday;

            int displayDays = isRewardAvailableToday ? totalClaimedDays : Mathf.Max(0, totalClaimedDays - 1);
            int displayCycleStartIndex = (displayDays / _config.UILoopDays) * _config.UILoopDays;

            for (int i = 0; i < _config.UILoopDays; i++)
            {
                int absoluteDayIndex = displayCycleStartIndex + i;
                QuestRewardConfig rewardForSlot = _config.GetRewardForTotalDays(absoluteDayIndex).RewardConfig;
                int localDisplayDay = (absoluteDayIndex % _config.UILoopDays) + 1;

                DailyLoginItemView viewInstance = _cachedViews[i];

                viewInstance.Initialize(absoluteDayIndex, rewardForSlot, $"Day {localDisplayDay}");

                if (absoluteDayIndex < totalClaimedDays)
                {
                    viewInstance.SetStateClaimed();
                }
                else if (absoluteDayIndex == totalClaimedDays)
                {
                    if (isRewardAvailableToday)
                    {
                        viewInstance.SetStateReadyToClaim();
                    }
                    else
                    {
                        viewInstance.SetStateLocked();
                    }
                }
                else
                {
                    viewInstance.SetStateLocked();
                }
            }
        }

        private void ProcessClaimRequest(int absoluteDayIndex)
        {
            QuestRewardConfig rewardConfig = _config.GetRewardForTotalDays(absoluteDayIndex).RewardConfig;

            if (rewardConfig.Category == RewardCategory.Gold || 
                rewardConfig.Category == RewardCategory.Gem || 
                rewardConfig.Category == RewardCategory.Energy)
            {
                int localIndex = absoluteDayIndex % _config.UILoopDays;
                DailyLoginItemView clickedView = _cachedViews[localIndex];
                
                _currencyFlightService.PlayFlightAnimation(
                    rewardConfig.Category, 
                    rewardConfig.Amount, 
                    clickedView.transform.position); 
            }
            _loginService.ClaimTodayReward();
        }

        private void HandleRewardClaimed(int newTotalDays)
        {
            if (_panelRoot.activeSelf)
            {
                RefreshPanelState();
            }
        }

        private void HandleNewDayAvailable()
        {
            if (_panelRoot.activeSelf)
            {
                RefreshPanelState();
            }
        }

        private void OnDestroy()
        {
            if (_isPanelCurrentlyOpen && _inputLockService != null)
            {
                _inputLockService.RemoveLock();
            }

            if (_loginService != null)
            {
                _loginService.OnRewardClaimed -= HandleRewardClaimed;
                _loginService.OnNewDayAvailable -= HandleNewDayAvailable;
            }

            foreach (var view in _cachedViews)
            {
                if (view != null)
                {
                    view.OnClaimClicked -= ProcessClaimRequest;
                }
            }
        }
    }
}