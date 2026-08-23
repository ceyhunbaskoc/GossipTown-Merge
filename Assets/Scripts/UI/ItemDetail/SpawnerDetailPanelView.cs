using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.ItemDetail
{
    public class SpawnerDetailPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _spawnerDetailPanel;
        [SerializeField] private TextMeshProUGUI _spawnerNameText;
        [SerializeField] private Transform _spawnerDetailSlotsContainer;
        [SerializeField] private Button _panelCloseButton;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        private void Awake()
        {
            _panelCloseButton.onClick.AddListener(_close);
        }

        public void Setup(string itemName)
        {
            _spawnerNameText.text = itemName;
        }
        
        public void AddSlotElement(Transform elementTransform)
        {
            elementTransform.SetParent(_spawnerDetailSlotsContainer, false);
        }

        public void Show()
        {
            _spawnerDetailPanel.SetActive(true);
            _popupAnimator.Show();
        }

        private void _close()
        {
            _popupAnimator.Hide(() =>
            { 
                _spawnerDetailPanel.SetActive(false);
            });
        }

        private void OnDestroy()
        {
            _panelCloseButton.onClick.RemoveListener(_close);
        }
    }
}