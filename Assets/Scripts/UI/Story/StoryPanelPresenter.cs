using System.Collections;
using System.Collections.Generic;
using Data.Story;
using Core.Services;
using Core.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Story
{
    public class StoryPanelPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private TextMeshProUGUI _chapterTitleText;
        
        [Header("Prefabs")]
        [SerializeField] private DialogueMessageView _leftMessagePrefab;
        [SerializeField] private DialogueMessageView _rightMessagePrefab;
        
        [Header("Controls")]
        [SerializeField] private FullscreenClickListener _fullscreenClickListener;
        [SerializeField] private GameObject _finishContainer;
        [SerializeField] private Button _finishButton;
        [SerializeField] private UIPopupAnimator _popupAnimator;

        private StoryService _storyService;
        private IInputLockService _inputLockService;
        
        private string _currentChapterTitle; 
        
        private bool _isPanelCurrentlyOpen = false;
        
        private readonly List<DialogueMessageView> _activeMessages = new List<DialogueMessageView>();

        private void Awake()
        {
            _fullscreenClickListener.OnScreenClicked += ProgressStory;
            _finishButton.onClick.AddListener(ClosePanel);
        }

        public void Initialize(StoryService storyService, IInputLockService inputLockService)
        {
            _storyService = storyService;
            _inputLockService = inputLockService;
        }

        public void OpenPanel()
        {
            if (!_isPanelCurrentlyOpen)
            {
                _inputLockService.AddLock();
                _isPanelCurrentlyOpen = true;
            }
            _finishContainer.SetActive(false);
            _fullscreenClickListener.enabled = true;
            _panelRoot.SetActive(true);
            _popupAnimator.Show();
            
            ProgressStory();
        }

        private void ProgressStory()
        {
            if (!_storyService.TryGetNextLine(out DialogueLine currentLine, out string chapterTitle))
            {
                ShowFinishState();
                return;
            }
            
            if (_currentChapterTitle != chapterTitle)
            {
                ClearExistingMessages();
                _currentChapterTitle = chapterTitle;
                
                if (_chapterTitleText != null)
                {
                    _chapterTitleText.text = _currentChapterTitle;
                }
            }
            
            DialogueMessageView prefabToUse = currentLine.IsPlayerSide ? _rightMessagePrefab : _leftMessagePrefab;
            DialogueMessageView messageInstance = Instantiate(prefabToUse, _contentContainer);
            
            messageInstance.Bind(currentLine);
            _activeMessages.Add(messageInstance);

            StartCoroutine(ScrollToBottomRoutine());

            if (currentLine.IsPausePoint)
            {
                ShowFinishState();
            }
        }

        private void ShowFinishState()
        {
            _fullscreenClickListener.enabled = false;
            _finishContainer.SetActive(true);
            
            StartCoroutine(ScrollToBottomRoutine());
        }

        private void ClosePanel()
        {
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
                _isPanelCurrentlyOpen = false;
            }
            _popupAnimator.Hide(() =>
            {
                _panelRoot.SetActive(false);
            });
            //ClearExistingMessages();
        }

        private IEnumerator ScrollToBottomRoutine()
        {
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();

            float duration = 0.25f;
            float elapsed = 0f;
            float startPos = _scrollRect.verticalNormalizedPosition;
            float targetPos = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, targetPos, t * (2f - t));
                yield return null;
            }

            _scrollRect.verticalNormalizedPosition = targetPos;
        }

        private void ClearExistingMessages()
        {
            foreach (var msg in _activeMessages)
            {
                if (msg != null) Destroy(msg.gameObject);
            }
            _activeMessages.Clear();
        }

        private void OnDestroy()
        {
            _fullscreenClickListener.OnScreenClicked -= ProgressStory;
            _finishButton.onClick.RemoveListener(ClosePanel);
            if (_isPanelCurrentlyOpen)
            {
                _inputLockService.RemoveLock();
            }
        }
    }
}