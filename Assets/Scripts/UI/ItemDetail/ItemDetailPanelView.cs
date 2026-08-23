using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.ItemDetail
{
    public class ItemDetailPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _itemDetailPanel;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private Transform _itemDetailSlotsContainer;
        [SerializeField] private Button _spawnerDetailButton;
        [SerializeField] private Button _panelCloseButton;
        [SerializeField] private Image _spawnerSlotElementImage;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        public event Action<string> OnSpawnerDetailRequested;

        private string _spawnerId;

        private void Awake()
        {
            _spawnerDetailButton.onClick.AddListener(_onSpawnerDetailRequestedClicked);
            _panelCloseButton.onClick.AddListener(_close);
        }

        public void Setup(string itemName, string spawnerId, Sprite spawnerIcon)
        {
            _itemNameText.text = itemName;
            _spawnerId = spawnerId;
            
            bool hasSpawner = !string.IsNullOrEmpty(spawnerId) && spawnerIcon != null;
            _spawnerDetailButton.gameObject.SetActive(hasSpawner);
            
            if (hasSpawner)
            {
                _setupSpawnerSlotElement(spawnerIcon);
            }
        }
        
        public void AddSlotElement(Transform elementTransform)
        {
            elementTransform.SetParent(_itemDetailSlotsContainer, false);
        }

        private void _setupSpawnerSlotElement(Sprite icon)
        {
            _spawnerSlotElementImage.sprite = icon;
        }

        public void Show()
        {
            _itemDetailPanel.SetActive(true);
            _popupAnimator.Show();
        }

        private void _close()
        {
            _popupAnimator.Hide(() =>
            { 
                _itemDetailPanel.SetActive(false);
            });
        }
        
        private void _onSpawnerDetailRequestedClicked()
        {
            OnSpawnerDetailRequested?.Invoke(_spawnerId);
        }

        private void OnDestroy()
        {
            _spawnerDetailButton.onClick.RemoveListener(_onSpawnerDetailRequestedClicked);
            _panelCloseButton.onClick.RemoveListener(_close);
        }
    }
}