using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Tutorial
{
    public class FreeSpawnerRefillPanelPresenter : MonoBehaviour
    {
        [Header("UI Elements")] 
        [SerializeField]
        private GameObject _panel;
        [SerializeField] 
        private Button _claimFreeRefillButton;
        [SerializeField] 
        private UIPopupAnimator _uiPopupAnimator;
        private Action _onClaimRequestedCallback;

        private void Awake()
        {
            _claimFreeRefillButton.onClick.AddListener(OnClaimButtonClicked);
            ClosePanel();
        }

        public void OpenPanel(Action onClaimRequested)
        {
            _onClaimRequestedCallback = onClaimRequested;

            _panel.SetActive(true);
            _uiPopupAnimator.Show();
        }

        public void ClosePanel()
        {
            _uiPopupAnimator.Hide(() =>
            {
                _panel.SetActive(false);
                _onClaimRequestedCallback = null;
            });
        }

        private void OnClaimButtonClicked()
        {
            _onClaimRequestedCallback?.Invoke();
        }

        private void OnDestroy()
        {
            if (_claimFreeRefillButton != null)
            {
                _claimFreeRefillButton.onClick.RemoveListener(OnClaimButtonClicked);
            }
        }
    }
}