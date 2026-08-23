using System;
using System.Collections.Generic;
using Core.Backpack;
using Core.GridSystem;
using Core.Services;
using Data;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Backpack
{
    public class BackpackPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject _panelUI;
        [SerializeField] private Transform _slotsContainer; 
        [SerializeField] private BackpackSlotView _slotPrefab; 
        [SerializeField] private Button _backpackButton; 
        [SerializeField] private Button _backpackCloseButton;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        private GridDataModel _backpackModel;
        private BoardTransferService _transferService;
        private ItemDatabaseSO _itemDatabase;
        private BackpackUnlockerService _backpackUnlockerService;
        private IWarningMessageService _warningMessageService;

        private readonly Dictionary<Vector2Int, BackpackSlotView> _slotRegistry = new Dictionary<Vector2Int, BackpackSlotView>();
        private int _currentUnlockGemCost;

        public void Initialize(GridDataModel backpackModel, 
            BoardTransferService transferService, 
            ItemDatabaseSO itemDatabase, 
            BackpackUnlockerService backpackUnlockerService,
            IWarningMessageService warningMessageService)
        {
            _backpackModel = backpackModel;
            _transferService = transferService;
            _itemDatabase = itemDatabase;
            _backpackUnlockerService = backpackUnlockerService;
            _warningMessageService = warningMessageService;
            
            GenerateGrid();
            RefreshAllSlots();

            _backpackModel.OnItemPlaced += HandleItemChanged;
            _backpackModel.OnCellCleared += HandleCellCleared;
            _backpackModel.OnCellUnlocked += UpdateForNextLocked;
            
            _backpackButton.onClick.AddListener(OpenPanel);
            _backpackCloseButton.onClick.AddListener(ClosePanel);
        }

        private void GenerateGrid()
        {
            if (_slotRegistry.Count > 0) return;

            for (int y = 0; y < _backpackModel.Height; y++)
            {
                for (int x = 0; x < _backpackModel.Width; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    BackpackSlotView slotView = Instantiate(_slotPrefab, _slotsContainer);
                    slotView.Bind(() => OnSlotClicked(pos), () => _onUnlockClicked(pos));
                    slotView.SetEmpty();
                    
                    _slotRegistry.Add(pos, slotView);
                }
            }
        }

        public void OpenPanel()
        {
            _panelUI.SetActive(true);
            _popupAnimator.Show();
            RefreshAllSlots(); 
        }

        public void ClosePanel()
        {
            _popupAnimator.Hide(() => 
            {
                _panelUI.SetActive(false);
            });
        }

        private void HandleItemChanged(Vector2Int pos, IGridItem item)
        {
            if (_panelUI.activeSelf) 
            {
                UpdateSlotState(pos);
            }
        }

        private void HandleCellCleared(Vector2Int pos)
        {
            if (_panelUI.activeSelf) 
            {
                UpdateSlotState(pos);
            }
        }

        private void UpdateForNextLocked(Vector2Int pos)
        {
            Vector2Int nextFirstLockedPos = _backpackModel.ToGridPosition(_backpackModel.ToFlatIndex(pos) + 1);
            _showNextFirstLockedOverlay(nextFirstLockedPos);
        }

        bool isFoundedFirstLocked = false;
        private void RefreshAllSlots()
        {
            isFoundedFirstLocked = false;
            foreach (var pos in _slotRegistry.Keys)
            {
                UpdateSlotState(pos);
                if (!isFoundedFirstLocked)
                {
                    _showNextFirstLockedOverlay(pos);
                }
            }
        }

        private void _showNextFirstLockedOverlay(Vector2Int pos)
        {
            if (!_slotRegistry.TryGetValue(pos, out BackpackSlotView slotView)) return;
            if (_backpackModel.IsCellLocked(pos))
            {
                _currentUnlockGemCost = _backpackUnlockerService.CalculateNextUnlockCost(_backpackModel.ToFlatIndex(pos));
                slotView.SetFirstLocked(_currentUnlockGemCost);
                isFoundedFirstLocked = true;
                Debug.Log($"Setting first locked {pos}");
            }
        }

        private void UpdateSlotState(Vector2Int pos)
        {
            if (!_slotRegistry.TryGetValue(pos, out BackpackSlotView slotView)) return;

            if (_backpackModel.IsCellLocked(pos))
            {
                slotView.SetLocked();
                return;
            }

            IGridItem item = _backpackModel.GetItemAt(pos);
            if (item != null)
            {
                Sprite icon = _itemDatabase.GetItemDef(item.Id).GetIcon(item.Level);
                if (icon == null) 
                {
                    Debug.LogError($"[Backpack] Error: {item.Id} item icon not found!");
                }
                
                slotView.SetItem(icon);
            }
            else
            {
                slotView.SetEmpty();
            }
        }

        private void OnSlotClicked(Vector2Int backpackPos)
        {
            if (!_transferService.TryRetrieveFromBackpack(backpackPos))
            {
                _warningMessageService.ShowWarning("Grid is full!", Input.mousePosition);
            }
        }

        private void _onUnlockClicked(Vector2Int pos)
        {
            if (_backpackUnlockerService.TryUnlockSlot(pos, _currentUnlockGemCost))
            {
                if (!_slotRegistry.TryGetValue(pos, out BackpackSlotView slotView)) return;
                slotView.SetEmpty();
            }
            else
            {
                _warningMessageService.ShowWarning("Not enough gems!", Input.mousePosition);
            }
        }

        private void OnDestroy()
        {
            _backpackModel.OnItemPlaced -= HandleItemChanged;
            _backpackModel.OnCellCleared -= HandleCellCleared;
            _backpackModel.OnCellUnlocked -= UpdateForNextLocked;
            
            _backpackButton.onClick.RemoveListener(OpenPanel);
            _backpackCloseButton.onClick.RemoveListener(ClosePanel);
        }
    }
}