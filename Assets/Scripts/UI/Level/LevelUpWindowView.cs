using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Level
{
    public class LevelUpWindowView : MonoBehaviour
    {
        [SerializeField] private GameObject _levelUpWindow;
        [SerializeField] private TextMeshProUGUI _newLevelText;
        [SerializeField] private Button _collectButton;
        [SerializeField] private Transform _rewardsContainer;
        [SerializeField] private UIPopupAnimator _popupAnimator;
        
        public Transform RewardsContainer => _rewardsContainer;
        public event Action OnCollectClicked;

        private void Awake()
        {
            _collectButton.onClick.AddListener(() => OnCollectClicked?.Invoke());
        }

        public void ShowLevelUpWindow()
        {
            _levelUpWindow.SetActive(true);
            _popupAnimator.Show();
        }

        public void HideLevelUpWindow()
        {
            _popupAnimator.Hide(() =>
            {
                _levelUpWindow.SetActive(false);
            });
        }

        public void SetLevelText(int level)
        {
            if (_newLevelText != null)
            {
                _newLevelText.text = $"LEVEL {level}";
            }
        }

        private void OnDestroy()
        {
            _collectButton.onClick.RemoveAllListeners();
        }
    }
}