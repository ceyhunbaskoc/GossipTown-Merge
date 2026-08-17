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
        private CanvasGroup _canvasGroup;
        private Coroutine _activeCoroutine;

        private void Awake()
        {
            _layoutElement = GetComponent<LayoutElement>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _layoutElement.flexibleWidth = 0; 
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            if (!gameObject.activeInHierarchy) return;
            _activeCoroutine = StartCoroutine(AnimateLayout(0f, _targetWidth, 0f, 1f, null));
        }

        public void Hide(Action onComplete = null)
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            
            if (!gameObject.activeInHierarchy)
            {
                onComplete?.Invoke();
                return;
            }

            _activeCoroutine = StartCoroutine(AnimateLayout(_layoutElement.preferredWidth, 0f, _canvasGroup.alpha, 0f, () => 
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

                _layoutElement.preferredWidth = Mathf.LerpUnclamped(startWidth, endWidth, curvedT);
                
                _canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, endAlpha, curvedT);

                yield return null;
            }

            _layoutElement.preferredWidth = endWidth;
            _canvasGroup.alpha = endAlpha;
            _activeCoroutine = null;

            onComplete?.Invoke();
        }
    }
}