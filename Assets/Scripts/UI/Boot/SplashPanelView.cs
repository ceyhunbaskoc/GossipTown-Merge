using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Boot
{
    public class SplashPanelView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.5f;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            _canvasGroup.gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
        }

        public void FadeOutAndHide(Action onComplete = null)
        {
            _canvasGroup.DOFade(0f, _fadeDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    _canvasGroup.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }
}