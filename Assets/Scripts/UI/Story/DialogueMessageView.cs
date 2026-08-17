using System.Collections;
using Data.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Story
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueMessageView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _messageText;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Bind(DialogueLine lineData)
        {
            if (lineData.Speaker != null)
            {
                _portraitImage.sprite = lineData.Speaker.Portrait;
                _nameText.text = lineData.Speaker.DisplayName;
            }

            _messageText.text = lineData.Text;

            if (lineData.IsPlayerSide)
            {
            }
            else
            {
            }

            StartCoroutine(PlayAppearAnimation());
        }
        private IEnumerator PlayAppearAnimation()
        {
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * 0.8f;
            
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float smoothT = t * (2f - t); 

                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothT);
                transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, smoothT);
                
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
        }
    }
}