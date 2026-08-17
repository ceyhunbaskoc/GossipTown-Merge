using Data;
using Core.Services;
using Core.GridSystem;
using UnityEngine;
using TMPro;
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
        [SerializeField] private Button _trashButton;
        
        private BoardSelectionService _selectionService;
        private GridDataModel _gridModel;

        public void Initialize(BoardSelectionService selectionService, GridDataModel gridModel)
        {
            _selectionService = selectionService;
            _gridModel = gridModel;
            
            _selectionService.OnItemSelected += ShowInfo;
            _selectionService.OnSelectionCleared += HideInfo;
            
            _trashButton.onClick.AddListener(OnTrashButtonClicked);
            
            HideInfo();
        }
        

        private void ShowInfo(IGridItem item, BaseItemDefinitionSO itemDef)
        {
            _panelContainer.SetActive(true);
            
            _itemNameText.text = $"{itemDef.ItemName} (Lvl {item.Level})";
            _itemDescriptionText.text = itemDef.GetDescription();
            
            bool isLocked = _gridModel.IsCellLocked(_selectionService.CurrentSelectedPosition.Value);
            _trashButton.gameObject.SetActive(!isLocked);
        }

        private void OnTrashButtonClicked()
        {
            if (_selectionService.CurrentSelectedPosition.HasValue)
            {
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