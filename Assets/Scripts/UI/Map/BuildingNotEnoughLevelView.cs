using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Map
{
    public class BuildingNotEnoughLevelView : MonoBehaviour
    {
        [SerializeField] private GameObject _notEnoughLevelPopup;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _requiredLevelText;
        [SerializeField] private TextMeshProUGUI _buildingNameText;
        [SerializeField] private Image _buildingIcon;
        [SerializeField] private UIPopupAnimator _popupAnimator;
        
        public event Action OnCloseClicked;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void SetContent(int requiredLevel, string buildingName, Sprite icon)
        {
            _requiredLevelText.text = $"You can't unlock this building.\nRequired Level: {requiredLevel}";
            _buildingNameText.text = buildingName;
            _buildingIcon.sprite = icon;
        }
        
        public void Open()
        {
            _notEnoughLevelPopup.SetActive(true);
            _popupAnimator.Show();
        }

        public void Close()
        {
            _popupAnimator.Hide(() =>
            {
                _notEnoughLevelPopup.SetActive(false);
            });
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
        }
    }
}