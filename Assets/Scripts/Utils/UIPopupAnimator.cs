using System;
using UnityEngine;
using DG.Tweening;

namespace Utils
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public class UIPopupAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 1f)] private float _animationDuration = 0.3f;
        
        [SerializeField] private Ease _showEase = Ease.OutBack;
        
        [SerializeField] private Ease _hideEase = Ease.InBack;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        
        private Sequence _animationSequence;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            
            _animationSequence?.Kill();

            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;

            _animationSequence = DOTween.Sequence();
            
            _animationSequence.SetUpdate(true)
                .Append(_rectTransform.DOScale(Vector3.one, _animationDuration).SetEase(_showEase))
                .Join(_canvasGroup.DOFade(1f, _animationDuration * 0.8f)) 
                .OnComplete(() => onComplete?.Invoke());
        }

        public void Hide(Action onComplete = null)
        {
            _animationSequence?.Kill();

            _animationSequence = DOTween.Sequence();
            
            _animationSequence.SetUpdate(true)
                .Append(_rectTransform.DOScale(Vector3.zero, _animationDuration * 0.8f).SetEase(_hideEase))
                .Join(_canvasGroup.DOFade(0f, _animationDuration * 0.8f))
                .OnComplete(() => 
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
        
        private void OnDestroy()
        {
            _animationSequence?.Kill();
        }
    }
}