using UnityEngine;
using DG.Tweening;
using Core.Services;
using Core.Controllers;
using Data;
using Core.GridSystem;

namespace Core.Views
{
    // SOLID (SRP): Yalnızca çerçevenin animasyonuyla ilgilenir. Item'lar bu sınıfı bilmez.
    public class BoardSelectionVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _selectionFrame;
        
        [Header("Settings")]
        [SerializeField] private float _animationDuration = 0.15f;
        [SerializeField] private float _samePositionPulseScale = 0.85f;

        private BoardSelectionService _selectionService;
        private BaseGridViewController _boardController;
        private Sequence _transitionSequence;
        
        private Vector2Int? _lastSelectedPosition;

        public void Initialize(BoardSelectionService selectionService, BaseGridViewController boardController)
        {
            _selectionService = selectionService;
            _boardController = boardController;

            _selectionService.OnItemSelected += HandleItemSelected;
            _selectionService.OnSelectionCleared += HandleSelectionCleared;
            _selectionService.OnVisualsSuspended += HandleVisualsSuspended;

            _selectionFrame.SetActive(false);
            _lastSelectedPosition = null;
        }

        private void HandleItemSelected(IGridItem item, BaseItemDefinitionSO itemDef)
        {
            if (_selectionService.IsVisualsSuspended) return; 
            
            AnimateFrameToCurrentSelection();
        }

        private void HandleVisualsSuspended(bool isSuspended)
        {
            _selectionFrame.transform.DOKill();
            _transitionSequence?.Kill();

            if (isSuspended)
            {
                _transitionSequence = DOTween.Sequence();
                _transitionSequence.Append(_selectionFrame.transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack));
                _transitionSequence.OnComplete(() => _selectionFrame.SetActive(false));
            }
            else
            {
                if (_selectionService.CurrentSelectedPosition.HasValue)
                {
                    AnimateFrameToCurrentSelection();
                }
            }
        }

        private void AnimateFrameToCurrentSelection()
        {
            if (!_selectionService.CurrentSelectedPosition.HasValue) return;

            Vector2Int currentGridPos = _selectionService.CurrentSelectedPosition.Value;
            Vector3 targetWorldPos = _boardController.GridToWorldPosition(currentGridPos);

            _selectionFrame.transform.DOKill();
            _transitionSequence?.Kill();
            _transitionSequence = DOTween.Sequence();

            if (_selectionFrame.activeSelf)
            {
                if (_lastSelectedPosition.HasValue && _lastSelectedPosition.Value == currentGridPos)
                {
                    float halfDuration = _animationDuration * 0.5f;
                    _transitionSequence.Append(_selectionFrame.transform.DOScale(Vector3.one * _samePositionPulseScale, halfDuration).SetEase(Ease.OutQuad));
                    _transitionSequence.Append(_selectionFrame.transform.DOScale(Vector3.one, halfDuration).SetEase(Ease.OutBounce));
                }
                else
                {
                    _transitionSequence.Append(_selectionFrame.transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack));
                    _transitionSequence.AppendCallback(() => _selectionFrame.transform.position = targetWorldPos);
                    _transitionSequence.Append(_selectionFrame.transform.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack));
                }
            }
            else
            {
                _selectionFrame.transform.position = targetWorldPos;
                _selectionFrame.SetActive(true);
                _selectionFrame.transform.localScale = Vector3.zero;
                _transitionSequence.Append(_selectionFrame.transform.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack));
            }
            _lastSelectedPosition = currentGridPos;
        }

        private void HandleSelectionCleared()
        {
            _selectionFrame.transform.DOKill();
            _transitionSequence?.Kill();
            
            _lastSelectedPosition = null;

            _transitionSequence = DOTween.Sequence();
            _transitionSequence.Append(_selectionFrame.transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack));
            _transitionSequence.OnComplete(() => _selectionFrame.SetActive(false));
        }

        private void OnDestroy()
        {
            if (_selectionService != null)
            {
                _selectionService.OnItemSelected -= HandleItemSelected;
                _selectionService.OnSelectionCleared -= HandleSelectionCleared;
                _selectionService.OnVisualsSuspended -= HandleVisualsSuspended;
            }
            _selectionFrame.transform.DOKill();
            _transitionSequence?.Kill();
        }
    }
}