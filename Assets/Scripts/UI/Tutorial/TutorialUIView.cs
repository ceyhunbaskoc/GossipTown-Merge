using System;
using Core.Tutorial;
using Data.Tutorial;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Tutorial
{
    public class TutorialUIView : MonoBehaviour, ITutorialUI
    {
        [Header("Core References")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private RectTransform _canvasRect; 
        
        [Header("Hole Punch System")]
        [SerializeField] private GameObject _overlayContainer;
        [SerializeField] private TutorialHolePunchUI _holePunchFilter;
        [SerializeField] private RectTransform _holeRect;
        [SerializeField] private Image _holeImage;
        
        [Header("Elements")]
        [SerializeField] private RectTransform _handCursor;
        [SerializeField] private CanvasGroup _handCanvasGroup;
        [SerializeField] private GameObject _coreLoopPanel;
        [SerializeField] private UIPopupAnimator _panelUIAnimator;
        [SerializeField] private Button _gotItButton;
        [SerializeField] private RectTransform _backPackButton;
        [SerializeField] private RectTransform _backToMenuButton;
        [SerializeField] private Button _nextButton;
        
        [Header("Hand Animation Tweaks")]
        [SerializeField] private Vector2 _handStartOffset = new Vector2(-30f, -60f); 
        [SerializeField] private Vector2 _handTargetOffset = new Vector2(0f, -15f);
        
        [Header("Hole Visual Tweaks")]
        [SerializeField] private Vector3 _worldSpaceHoleOffset = new Vector3(0f, -0.2f, 0f);

        [Header("Mentor")] 
        [SerializeField] private GameObject _mentorContainer;
        [SerializeField] private TextMeshProUGUI _mentorText;

        private Action _onTutorialFinishedCallback;
        private Sequence _handAnimationSequence;
        private Tween _holeMoveTween;
        private Tween _holeSizeTween;

        public event Action OnNextButtonClicked;

        private void Awake()
        {
            _gotItButton.onClick.AddListener(OnGotItClicked);
            
            _holePunchFilter.SetTargetHole(_holeRect);
            _nextButton.onClick.AddListener(_onNextButtonClicked);
            
            ClearHighlights();
        }

        private void MoveAndResizeHoleLocal(Vector2 localPos, Vector2 targetSize)
        {
            _holeMoveTween?.Kill();
            _holeSizeTween?.Kill();

            if (_holeRect.sizeDelta == Vector2.zero)
            {
                _holeRect.anchoredPosition = localPos;
                _holeRect.sizeDelta = targetSize;
            }
            else
            {
                _holeMoveTween = _holeRect.DOAnchorPos(localPos, 0.5f).SetEase(Ease.OutQuint);
                _holeSizeTween = _holeRect.DOSizeDelta(targetSize, 0.5f).SetEase(Ease.OutQuint);
            }
        }

        public void HighlightGridCells(Vector3 worldPos1, Vector3 worldPos2, float cellSize)
        {
            _overlayContainer.SetActive(true);
            _nextButton.gameObject.SetActive(false);
            float halfSize = cellSize / 2f;

            Vector3 offsetPos1 = worldPos1 + _worldSpaceHoleOffset;
            Vector3 offsetPos2 = worldPos2 + _worldSpaceHoleOffset;

            Vector3 minWorld = new Vector3(
                Mathf.Min(offsetPos1.x, offsetPos2.x) - halfSize,
                Mathf.Min(offsetPos1.y, offsetPos2.y) - halfSize, 0f);

            Vector3 maxWorld = new Vector3(
                Mathf.Max(offsetPos1.x, offsetPos2.x) + halfSize,
                Mathf.Max(offsetPos1.y, offsetPos2.y) + halfSize, 0f);

            Vector2 minScreen = _mainCamera.WorldToScreenPoint(minWorld);
            Vector2 maxScreen = _mainCamera.WorldToScreenPoint(maxWorld);

            RectTransform parentRect = (RectTransform)_holeRect.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, minScreen, _mainCamera, out Vector2 minLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, maxScreen, _mainCamera, out Vector2 maxLocal);

            Vector2 localCenter = (minLocal + maxLocal) / 2f;
            float localWidth = Mathf.Abs(maxLocal.x - minLocal.x);
            float localHeight = Mathf.Abs(maxLocal.y - minLocal.y);

            MoveAndResizeHoleLocal(localCenter, new Vector2(localWidth, localHeight));
        }

        public void SetMentorText(string text)
        {
            _mentorContainer.SetActive(true);
            _mentorText.text = text;
        }

        public void PlayHandAnimation(Vector3 startWorldPos, Vector3 endWorldPos)
        {
            _handCursor.gameObject.SetActive(true);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, _mainCamera.WorldToScreenPoint(startWorldPos), _mainCamera, out Vector2 startLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, _mainCamera.WorldToScreenPoint(endWorldPos), _mainCamera, out Vector2 endLocal);

            _handCursor.anchoredPosition = startLocal;

            _handAnimationSequence?.Kill();
            
            _handAnimationSequence = DOTween.Sequence();
            _handAnimationSequence.Append(_handCursor.DOAnchorPos(endLocal, 1f).SetEase(Ease.InOutSine))
                                  .Append(_handCanvasGroup.DOFade(0, 0.2f))
                                  .AppendCallback(() => _handCursor.anchoredPosition = startLocal)
                                  .Append(_handCanvasGroup.DOFade(1, 0.2f))
                                  .AppendInterval(0.3f)
                                  .SetLoops(-1);
        }
        
        private void PlayHandClickAnimation(Vector2 targetLocalPos)
        {
            _handCursor.gameObject.SetActive(true);
            _handCanvasGroup.alpha = 1f;
            
            _handAnimationSequence?.Kill();

            Vector2 finalTargetPos = targetLocalPos + _handTargetOffset;
            Vector2 startPos = finalTargetPos + _handStartOffset;
            _handCursor.anchoredPosition = startPos;
            _handAnimationSequence = DOTween.Sequence();
            _handAnimationSequence.Append(_handCursor.DOAnchorPos(finalTargetPos, 0.5f).SetEase(Ease.InOutSine))
                .Append(_handCursor.DOAnchorPos(startPos, 0.5f).SetEase(Ease.InOutSine))
                .SetLoops(-1);
        }

        public void HighlightOrderCompleteButton(RectTransform targetRect)
        {
            _nextButton.gameObject.SetActive(false);
            _overlayContainer.SetActive(true);

            Vector3[] worldCorners = new Vector3[4];
            targetRect.GetWorldCorners(worldCorners); 
    
            Vector2 screenBottomLeft = _mainCamera.WorldToScreenPoint(worldCorners[0]);
            Vector2 screenTopLeft = _mainCamera.WorldToScreenPoint(worldCorners[1]);
            Vector2 screenTopRight = _mainCamera.WorldToScreenPoint(worldCorners[2]);

            Vector2 centerScreenPos = (screenBottomLeft + screenTopRight) / 2f;
            
            float exactWidth = Mathf.Abs(screenTopRight.x - screenBottomLeft.x);
            float exactHeight = Mathf.Abs(screenTopLeft.y - screenBottomLeft.y);

            float uiPadding = 20f; 

            MoveAndResizeHole(centerScreenPos, new Vector2(exactWidth + uiPadding, exactHeight + uiPadding));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, centerScreenPos, _mainCamera, out Vector2 localCenterPos);
            
            PlayHandClickAnimation(localCenterPos);
        }

        public void HighlightBackpackButton()
        {
            HighlightButton(_backPackButton);
        }

        public void HighlightBackToMenuButton()
        {
            HighlightButton(_backToMenuButton);
        }

        public void HighlightButton(RectTransform targetRect)
        {
            _overlayContainer.SetActive(true);
            _handCursor.gameObject.SetActive(false);
            _holeImage.raycastTarget = true;
            _nextButton.gameObject.SetActive(true);
            
            Vector3[] worldCorners = new Vector3[4];
            targetRect.GetWorldCorners(worldCorners); 
    
            Vector2 screenBottomLeft = _mainCamera.WorldToScreenPoint(worldCorners[0]);
            Vector2 screenTopLeft = _mainCamera.WorldToScreenPoint(worldCorners[1]);
            Vector2 screenTopRight = _mainCamera.WorldToScreenPoint(worldCorners[2]);

            Vector2 centerScreenPos = (screenBottomLeft + screenTopRight) / 2f;
            
            float exactWidth = Mathf.Abs(screenTopRight.x - screenBottomLeft.x);
            float exactHeight = Mathf.Abs(screenTopLeft.y - screenBottomLeft.y);

            float uiPadding = 20f; 

            MoveAndResizeHole(centerScreenPos, new Vector2(exactWidth + uiPadding, exactHeight + uiPadding));
        }

        private void _onNextButtonClicked()
        {
            OnNextButtonClicked?.Invoke();
        }

        private void MoveAndResizeHole(Vector2 screenPos, Vector2 targetSize)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, _mainCamera, out Vector2 localPos);

            _holeMoveTween?.Kill();
            _holeSizeTween?.Kill();

            if (_holeRect.sizeDelta == Vector2.zero)
            {
                _holeRect.anchoredPosition = localPos;
                _holeRect.sizeDelta = targetSize;
            }
            else
            {
                _holeMoveTween = _holeRect.DOAnchorPos(localPos, 0.5f).SetEase(Ease.OutQuint);
                _holeSizeTween = _holeRect.DOSizeDelta(targetSize, 0.5f).SetEase(Ease.OutQuint);
            }
        }

        public void ShowCoreLoopPanel(Action onComplete)
        {
            ClearHighlights();
            _onTutorialFinishedCallback = onComplete;
            _coreLoopPanel.SetActive(true);
            _panelUIAnimator.Show();
        }

        public void ClearHighlights()
        {
            _overlayContainer.SetActive(false);
            _holeImage.raycastTarget = false;
            _handCursor.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            _mentorContainer.SetActive(false);
            _panelUIAnimator.Hide(() =>
            {
                _coreLoopPanel.SetActive(false);
            });
            
            
            _holeRect.sizeDelta = Vector2.zero; 
            
            _handAnimationSequence?.Kill();
            _holeMoveTween?.Kill();
            _holeSizeTween?.Kill();
        }

        private void OnGotItClicked()
        {
            ClearHighlights();
            _onTutorialFinishedCallback?.Invoke();
        }

        private void OnDestroy()
        {
            _gotItButton.onClick.RemoveListener(OnGotItClicked);
            _handAnimationSequence?.Kill();
            _holeMoveTween?.Kill();
            _holeSizeTween?.Kill();
            _nextButton.onClick.RemoveListener(_onNextButtonClicked);
        }
    }
}