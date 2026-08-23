using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(LayoutElement))]
    [RequireComponent(typeof(CanvasGroup))]
    public class AnimatedLayoutElement : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.4f;
        [SerializeField] private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Target Dimensions")]
        [SerializeField] private float _targetWidth = 150f;

        private LayoutElement _layoutElement;
        private LayoutElement LayoutElement => _layoutElement != null ? _layoutElement : (_layoutElement = GetComponent<LayoutElement>());

        private CanvasGroup _canvasGroup;
        private CanvasGroup CanvasGroup => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        private Coroutine _activeCoroutine;

        private void Awake()
        {
            _layoutElement = LayoutElement;
            _canvasGroup = CanvasGroup;
            
            LayoutElement.flexibleWidth = 0; 
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            
            if (!gameObject.activeInHierarchy)
            {
                LayoutElement.preferredWidth = _targetWidth;
                CanvasGroup.alpha = 1f;
                return;
            }

            _activeCoroutine = StartCoroutine(AnimateLayout(LayoutElement.preferredWidth, _targetWidth, CanvasGroup.alpha, 1f, null));
        }

        public void Hide(Action onComplete = null)
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
    
            if (!gameObject.activeInHierarchy)
            {
                LayoutElement.preferredWidth = 0f;
                CanvasGroup.alpha = 0f;
                onComplete?.Invoke();
                return;
            }

            _activeCoroutine = StartCoroutine(AnimateLayout(LayoutElement.preferredWidth, 0f, CanvasGroup.alpha, 0f, () => 
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }));
        }

        private IEnumerator AnimateLayout(float startWidth, float endWidth, float startAlpha, float endAlpha, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _animationDuration);
                float curvedT = _easeCurve.Evaluate(t);

                LayoutElement.preferredWidth = Mathf.LerpUnclamped(startWidth, endWidth, curvedT);
                CanvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, endAlpha, curvedT);

                yield return null;
            }

            LayoutElement.preferredWidth = endWidth;
            CanvasGroup.alpha = endAlpha;
            _activeCoroutine = null;

            onComplete?.Invoke();
        }
    }
}