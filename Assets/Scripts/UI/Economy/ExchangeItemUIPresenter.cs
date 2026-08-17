using System;
using Core.Economy;
using Core.Economy.Exchange;
using Data;
using Data.Quests;
using TMPro;
using UI.Components;
using UI.FlightSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Economy
{
    public class ExchangeItemUIPresenter : MonoBehaviour
    {
        [Header("Icons")] 
        [SerializeField] private Transform _forwardIcon;
        [SerializeField] private Transform _reverseIcon;
        
        [Header("Selection Tabs")]
        [SerializeField] private Button _selectForwardButton;
        [SerializeField] private Button _selectReverseButton;

        [Header("Main Display")]
        [SerializeField] private TextMeshProUGUI _forwardText;
        [SerializeField] private TextMeshProUGUI _reverseText;
        
        [Header("Action")]
        [SerializeField] private Button _executeExchangeButton;

        private ICurrencyExchangeService _exchangeService;
        private CurrencyFlightService _currencyFlightService;
        private IWarningMessageService _warningMessageService;
        private Transform _currentRewardIconTransform;
        private ExchangePairSO _exchangePair;

        private ExchangeDirection _currentDirection = ExchangeDirection.Forward;

        public void Initialize(ExchangePairSO exchangePair, ICurrencyExchangeService exchangeService, CurrencyFlightService currencyFlightService, IWarningMessageService warningMessageService)
        {
            _exchangeService = exchangeService;
            _currencyFlightService = currencyFlightService;
            _warningMessageService = warningMessageService;
            _exchangePair = exchangePair;
            
            _selectForwardButton.onClick.AddListener(() => SetDirection(ExchangeDirection.Forward));
            _selectReverseButton.onClick.AddListener(() => SetDirection(ExchangeDirection.Reverse));
            
            _executeExchangeButton.onClick.AddListener(OnExecuteExchangeClicked);
            SetDirection(ExchangeDirection.Forward);
        }

        private void SetDirection(ExchangeDirection newDirection)
        {
            _currentDirection = newDirection;
            _currentRewardIconTransform = newDirection == ExchangeDirection.Forward ? _reverseIcon : _forwardIcon;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_exchangePair == null) return;

            int cost = _currentDirection == ExchangeDirection.Forward ? _exchangePair.ForwardCostAmount : _exchangePair.ReverseCostAmount;
            int reward = _currentDirection == ExchangeDirection.Forward ? _exchangePair.ForwardRewardAmount : _exchangePair.ReverseRewardAmount;

            if (_currentDirection == ExchangeDirection.Forward)
            {
                _forwardText.text = $"<color=red>-{cost}</color>";
                _reverseText.text = $"<color=white>+{reward}</color>";
            }
            else
            {
                _forwardText.text = $"<color=white>+{reward}</color>";
                _reverseText.text = $"<color=red>-{cost}</color>";
            }
        }

        private void OnExecuteExchangeClicked()
        {
            _exchangeService.TryExchange(
                _exchangePair,
                _currentDirection,
                onSuccess: () =>
                {
                    CurrencyType rewardCurrency = _currentDirection == ExchangeDirection.Forward ? _exchangePair.CurrencyB : _exchangePair.CurrencyA;
                    int rewardAmount = _currentDirection == ExchangeDirection.Forward ? _exchangePair.ForwardRewardAmount : _exchangePair.ReverseRewardAmount;
                    
                    RewardCategory rewardCategory = MapCurrencyToRewardCategory(rewardCurrency);
                    _currencyFlightService.PlayFlightAnimation(rewardCategory, rewardAmount, _currentRewardIconTransform.position);
                },
                onFailure: () => 
                {
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(_executeExchangeButton.transform.position);
                    _warningMessageService.ShowWarning("Insufficient balance", screenPos);
                }
            );
        }

        private RewardCategory MapCurrencyToRewardCategory(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.Energy => RewardCategory.Energy,
                CurrencyType.Gem => RewardCategory.Gem,
                _ => throw new ArgumentOutOfRangeException(nameof(currencyType), currencyType, $"[ExchangePresenter] Unsupported currency: {currencyType}")
            };
        }

        private void OnDestroy()
        {
            if (_selectForwardButton != null) _selectForwardButton.onClick.RemoveAllListeners();
            if (_selectReverseButton != null) _selectReverseButton.onClick.RemoveAllListeners();
            if (_executeExchangeButton != null) _executeExchangeButton.onClick.RemoveAllListeners();
        }
    }
}