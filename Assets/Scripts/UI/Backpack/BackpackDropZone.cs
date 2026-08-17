using Core;
using Core.Controllers;
using Core.GridSystem;
using Core.Services;
using DG.Tweening;
using UI.Components;
using UnityEngine;

namespace UI.Backpack
{
    public class BackpackDropZone : MonoBehaviour, IDropRequestReceiver, IDropHoverReceiver
    {
        [Header("Animation Targets")]
        [SerializeField] private RectTransform _targetAnimationAnchor;

        private BoardTransferService _transferService;
        private MainBoardController _mainBoardController;
        private IWarningMessageService _warningService;

        private Tween _hoverTween;

        public void Initialize(BoardTransferService transferService, MainBoardController mainBoardController, IWarningMessageService warningService)
        {
            _transferService = transferService;
            _mainBoardController = mainBoardController;
            _warningService = warningService;
        }

        public void OnHoverEnter(IViewItem viewItem)
        {
            if (_transferService.CanSendToBackpack(viewItem.CurrentGridPosition))
            {
                _hoverTween?.Kill();
                _hoverTween = _targetAnimationAnchor.DOScale(1.15f, 0.25f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        public void OnHoverExit(IViewItem viewItem)
        {
            _hoverTween?.Kill();
            _targetAnimationAnchor.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }

        public void OnItemDropRequested(IViewItem viewItem, Vector3 dropWorldPosition)
        {
            Vector2Int sourcePos = viewItem.CurrentGridPosition;

            if (!_transferService.CanSendToBackpack(sourcePos))
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(dropWorldPosition);
                _warningService.ShowWarning("Backpack is Full!", screenPos);
                viewItem.SnapBackToStart();
                return;
            }

            IViewItem flyingVisual = _mainBoardController.ExtractVisualAt(sourcePos);

            if (_transferService.TrySendToBackpack(sourcePos))
            {
                if (flyingVisual is DraggableItem draggableItem)
                {
                    draggableItem.transform.SetParent(null);
            
                    RectTransform targetRect = _targetAnimationAnchor != null ? _targetAnimationAnchor : GetComponent<RectTransform>();
                    Vector3 targetWorldPosition = GetWorldPositionOfUI(targetRect);
            
                    draggableItem.AnimateIntoUI(targetWorldPosition, () =>
                    {
                        draggableItem.gameObject.SetActive(false); 
                    });
                }
            }
        }

        private Vector3 GetWorldPositionOfUI(RectTransform rectTransform)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector3 screenPos = rectTransform.position;
                screenPos.z = Mathf.Abs(Camera.main.transform.position.z); 
                return Camera.main.ScreenToWorldPoint(screenPos);
            }
            
            return rectTransform.position;
        }
    }
}