using Core.Controllers;
using Core.Economy;
using Data;
using Core.Services;
using Core.GridSystem;
using Data.Quests;
using UnityEngine;
using TMPro;
using UI.FlightSystem;
using UnityEngine.UI;

namespace UI.Info
{
    public class ItemInfoPanelController : MonoBehaviour
    {
        [Header("Generic UI References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemDescriptionText;
        
        [Header("Global Actions")]
        [SerializeField] private GameObject _sellContainer;
        [SerializeField] private Button _sellButton;

        [SerializeField] private TextMeshProUGUI _sellPriceText;
        
        private BoardSelectionService _selectionService;
        private GridDataModel _gridModel;
        private IEconomyModifier _economyModifier;
        private CurrencyFlightService _currencyFlightService;
        private int _itemTempSellPrice = 0;
        private BaseGridViewController _gridViewController;

        public void Initialize(BoardSelectionService selectionService, 
            GridDataModel gridModel,
            IEconomyModifier economyModifier, 
            CurrencyFlightService currencyFlightService,
            BaseGridViewController gridViewController)
        {
            _selectionService = selectionService;
            _gridModel = gridModel;
            _economyModifier = economyModifier;
            _currencyFlightService = currencyFlightService;
            _gridViewController = gridViewController;
            
            _selectionService.OnItemSelected += ShowInfo;
            _selectionService.OnSelectionCleared += HideInfo;
            
            _sellButton.onClick.AddListener(OnSellButtonClicked);
            
            HideInfo();
        }
        

        private void ShowInfo(IGridItem item, BaseItemDefinitionSO itemDef)
        {
            _panelContainer.SetActive(true);
            _itemTempSellPrice = 0;
            
            _itemNameText.text = $"{itemDef.ItemName} (Lvl {item.Level})";
            _itemDescriptionText.text = itemDef.GetDescription();
            
            bool isLocked = _gridModel.IsCellLocked(_selectionService.CurrentSelectedPosition.Value);
            bool isSellable = itemDef.IsSellable;
            bool canShowSellButton = !isLocked && isSellable;
            _sellContainer.SetActive(canShowSellButton);
            if (canShowSellButton)
            {
                _itemTempSellPrice = itemDef.GetSellPrice(item.Level); 
                _sellPriceText.text = $"{_itemTempSellPrice}";
            }
        }

        private void OnSellButtonClicked()
        {
            if (_selectionService.CurrentSelectedPosition.HasValue)
            {
                Vector3 startWorldPosition = _gridViewController.GridToWorldPosition(_selectionService.CurrentSelectedPosition.Value);
                _currencyFlightService.PlayFlightAnimation(RewardCategory.Gold, _itemTempSellPrice, startWorldPosition);
                _economyModifier.AddGold(_itemTempSellPrice);
                _gridModel.TryClearCell(_selectionService.CurrentSelectedPosition.Value);
            }
        }

        private void HideInfo()
        {
            _panelContainer.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_selectionService != null)
            {
                _selectionService.OnItemSelected -= ShowInfo;
                _selectionService.OnSelectionCleared -= HideInfo;
            }
        }
    }
}