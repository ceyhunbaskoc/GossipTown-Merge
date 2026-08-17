using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Backpack
{
    public class BackpackSlotView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _slotButton;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private GameObject _lockedIcon;
        [SerializeField] private GameObject _lockedBackground;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private TextMeshProUGUI _unlockGemCost;
        [SerializeField] private Button _unlockButton;
        private Action _onClickAction;
        private Action _onUnlockClickAction;

        private void Awake()
        {
            _slotButton.onClick.AddListener(() => _onClickAction?.Invoke());
            _unlockButton.onClick.AddListener(() => _onUnlockClickAction?.Invoke());
        }

        public void Bind(Action onClickAction, Action onUnlockClickAction)
        {
            _onClickAction = onClickAction;
            _onUnlockClickAction = onUnlockClickAction;
        }

        public void SetEmpty()
        {
            _itemIcon.gameObject.SetActive(false);
            if (_lockedOverlay != null) _lockedOverlay.SetActive(false);
            if (_lockedBackground != null) _lockedBackground.SetActive(false);
            _slotButton.interactable = false;
        }

        public void SetLocked()
        {
            _itemIcon.gameObject.SetActive(false);
            if (_lockedBackground != null) _lockedBackground.SetActive(false);
            if (_lockedBackground != null) _lockedBackground.SetActive(true);
            _lockedIcon.SetActive(true);
            _slotButton.interactable = false;
        }

        public void SetFirstLocked(int gemCost)
        {
            _itemIcon.gameObject.SetActive(false);
            _lockedIcon.SetActive(false);
            _unlockGemCost.text = gemCost.ToString();
            if (_lockedOverlay != null) _lockedOverlay.SetActive(true);
            _slotButton.interactable = false;
        }

        public void SetItem(Sprite iconSprite)
        {
            _itemIcon.sprite = iconSprite;
            _itemIcon.gameObject.SetActive(true);
            if (_lockedOverlay != null) _lockedOverlay.SetActive(false);
            _slotButton.interactable = true;
        }
    }
}