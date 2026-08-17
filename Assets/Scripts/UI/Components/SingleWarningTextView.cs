using System.Collections;
using Core.Services;
using Data.UI;
using TMPro;
using UnityEngine;

namespace UI.Components
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SingleWarningTextView : MonoBehaviour
    {
        private TextMeshProUGUI _tmpText;
        private RectTransform _rectTransform;
        private Canvas _parentCanvas;
        
        private Coroutine _activeCoroutine;

        public void Initialize()
        {
            _tmpText = GetComponent<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();
            
            gameObject.SetActive(false);
        }

        public void PlayWarning(string message, Vector2 screenPosition, FloatingTextConfigSO config, InteractionResult result = InteractionResult.None)
        {
            if (config == null)
            {
                return;
            }
        
            gameObject.SetActive(true);
            _tmpText.text = message;

            Color selectedColor = Color.white;
            if (result == InteractionResult.AwesomeSuccess)
            {
                selectedColor = config.AwesomeColor;
            }
            else
            {
                selectedColor = config.DefaultColor;
            }
            _tmpText.color = selectedColor;
        
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
            RectTransform parentRect = _rectTransform.parent as RectTransform;
            
            Camera uiCamera = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;
        
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, 
                screenPosition, 
                uiCamera, 
                out Vector2 localPoint);
        
            float randomX = Random.Range(config.RandomXOffsetRange.x, config.RandomXOffsetRange.y);
            localPoint.x += randomX;
        
            _rectTransform.anchoredPosition = localPoint;
            
            _activeCoroutine = StartCoroutine(AnimateRoutine(localPoint, config));
        }

        private IEnumerator AnimateRoutine(Vector2 startPos, FloatingTextConfigSO config)
        {
            float elapsed = 0f;
            Vector2 endPos = startPos + (Vector2.up * config.RiseDistance);
            Color initialColor = _tmpText.color;

            while (elapsed < config.Duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / config.Duration);

                float moveT = config.MoveYCurve.Evaluate(normalizedTime);
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, moveT);

                float alphaT = config.AlphaCurve.Evaluate(normalizedTime);
                initialColor.a = alphaT;
                _tmpText.color = initialColor;

                if (config.UseScalePulse)
                {
                    float scaleT = config.ScaleCurve.Evaluate(normalizedTime);
                    _rectTransform.localScale = Vector3.one * scaleT;
                }

                yield return null;
            }

            gameObject.SetActive(false);
            _activeCoroutine = null;
        }
    }
}