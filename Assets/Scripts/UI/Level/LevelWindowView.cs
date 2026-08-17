using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Level
{
    public class LevelWindowView : MonoBehaviour
    {
        [SerializeField] private GameObject _windowView;
        [SerializeField] private Transform _scrollContentParent;
        [SerializeField] private Button _closeButton;
        
        [SerializeField] private Slider _windowProgressSlider;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        public Transform ScrollContentParent => _scrollContentParent;
        public event Action OnCloseClicked;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void Show()
        {
            _windowView.SetActive(true);
            _popupAnimator.Show();
        }

        public void Hide()
        {
            _popupAnimator.Hide(() =>
            {
                _windowView.SetActive(false);
            });
        }
        
        public void UpdateProgress(float progressRatio)
        {
            if (_windowProgressSlider != null)
            {
                _windowProgressSlider.value = progressRatio;
            }
        }
        public void SetFirstSiblingSlider()
        {
            if (_windowProgressSlider != null)
            {
                _windowProgressSlider.transform.SetAsFirstSibling();
            }
        }
    }
}