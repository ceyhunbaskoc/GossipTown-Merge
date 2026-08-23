using System;
using System.Collections.Generic;
using Core.Controllers;
using Core.GridSystem;
using Core.PoolSystem;
using Core.Rules;
using Core.Views;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core
{
    public class DraggableItem : MonoBehaviour, IViewItem, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPoolable, IDropHoverReceiver
    {
        [Header("Dependencies")]
        [SerializeField] private ItemVisualizer _visualizer;
        public IGridItem Data { get; private set; } 
        public Vector2Int CurrentGridPosition { get; set; }

        private IDropRequestReceiver _dropReceiver;
        private IInteractRequestReceiver _interactRequestReceiver;
        private Vector3 _startPosition;
        private Camera _mainCamera;
        
        private bool _isLockedInCell;
        private bool _isAnimating;
        private Tween _movementTween;
        private IDropHoverReceiver _currentHoverTarget;
        
        private IMergeValidator _mergeValidator;
        
        public ItemVisualizer Visualizer => _visualizer;
        public void SetLockedState(bool isLocked)
        {
            _isLockedInCell = isLocked;
        }
        
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void Initialize(IGridItem data, IDropRequestReceiver dropReceiver, IInteractRequestReceiver interactReceiver, IMergeValidator mergeValidator, Sprite icon)
        {
            Data = data;
            _dropReceiver = dropReceiver;
            _interactRequestReceiver = interactReceiver;
            _mergeValidator = mergeValidator;
            
            bool isSpawner = Data is SpawnerItemData;
            
            if (isSpawner)
            {
                var spawnerData = (SpawnerItemData)Data;
                spawnerData.OnCooldownStateChanged += _handleCooldownState;
                _handleCooldownState(spawnerData.IsInCooldown);
            }
            
            _visualizer.SetVisual(icon);
            _visualizer.SetBadgeActive(isSpawner); 
            
            _visualizer.PlaySpawnAnimation();
        }
        
        private void _handleCooldownState(bool isCoolingDown)
        {
            if (isCoolingDown) _visualizer.SetColor(Color.gray); 
            else _visualizer.SetColor(Color.white); 
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isLockedInCell || _isAnimating)
            {
                eventData.pointerDrag = null; 
                return;
            }
            
            _startPosition = transform.position;
            _visualizer.PlayPickUpAnimation();
            _visualizer.BringToFront();
            
            if (_interactRequestReceiver is MainBoardController boardController)
            {
                boardController.SelectionService.SelectItemAt(CurrentGridPosition);
                boardController.SelectionService.SetVisualsSuspended(true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
            worldPosition.z = 0f;
            transform.position = worldPosition;

            IDropHoverReceiver hoverReceiver = FindHoverReceiverUnderPointer(eventData);
    
            if (hoverReceiver != _currentHoverTarget)
            {
                _currentHoverTarget?.OnHoverExit(this);
                _currentHoverTarget = hoverReceiver;
                _currentHoverTarget?.OnHoverEnter(this);
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            _currentHoverTarget?.OnHoverExit(this);
            _currentHoverTarget = null;
    
            _visualizer.PlayDropAnimation();
            var boardController = _interactRequestReceiver as MainBoardController;
            IDropRequestReceiver targetReceiver = FindDropReceiverUnderPointer(eventData);

            if (targetReceiver != null)
            {
                targetReceiver.OnItemDropRequested(this, transform.position);
            }
            else if (_dropReceiver != null) 
            {
                _dropReceiver.OnItemDropRequested(this, transform.position);
            }
            else
            {
                SnapBackToStart();
            }
            boardController?.SelectionService.SetVisualsSuspended(false);
        }
        
        private T FindComponentUnderPointer<T>(PointerEventData eventData) where T : class
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent(out T component))
                {
                    if (ReferenceEquals(component, this)) 
                    {
                        continue;
                    }
                    
                    return component;
                }
            }
            return null;
        }

        private IDropRequestReceiver FindDropReceiverUnderPointer(PointerEventData eventData) 
            => FindComponentUnderPointer<IDropRequestReceiver>(eventData);

        private IDropHoverReceiver FindHoverReceiverUnderPointer(PointerEventData eventData) 
            => FindComponentUnderPointer<IDropHoverReceiver>(eventData);
        
        public void AnimateIntoUI(Vector3 targetUIPosition, Action onComplete)
        {
            _isAnimating = true;
            _visualizer.BringToFront();
    
            _movementTween?.Kill();
            transform.DOKill();

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOScale(Vector3.one * 1.15f, 0.15f).SetEase(Ease.OutQuad));

            seq.Append(transform.DOMove(targetUIPosition, 0.35f).SetEase(Ease.InBack));
            seq.Join(transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InQuad));
            seq.Join(transform.DORotate(new Vector3(0, 0, 180), 0.35f, RotateMode.FastBeyond360).SetEase(Ease.InQuad));

            seq.OnComplete(() =>
            {
                _isAnimating = false;
                onComplete?.Invoke();
            });
        }
        
        public void SnapBackToStart()
        {
            _isAnimating = true;
            _movementTween?.Kill();
            
            _movementTween = transform.DOMove(_startPosition, 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => 
                {
                    _visualizer.ResetSortingOrder();
                    _isAnimating = false;
                });
        }
        
        public void MoveToWorldPosition(Vector3 targetPosition)
        {
            _isAnimating = true;
            _visualizer.ResetSortingOrder();
            _movementTween?.Kill();
            
            _movementTween = transform.DOMove(targetPosition, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => _isAnimating = false);
        }

        public void MoveWithArcTo(Vector3 targetPosition)
        {
            _isAnimating = true;
            _movementTween?.Kill();
            
            _movementTween = transform.DOJump(targetPosition, jumpPower: 1f, numJumps: 1, duration: 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => _isAnimating = false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isLockedInCell || _isAnimating) return;
            if (eventData.dragging) return;
            
            _visualizer.PlayClickAnimation();
            
            if (_interactRequestReceiver is MainBoardController boardController)
            {
                boardController.SelectionService.SelectItemAt(CurrentGridPosition);
            }
    
            if (_interactRequestReceiver != null)
            {
                _interactRequestReceiver.OnItemInteractRequested(CurrentGridPosition, Data);
            }
        }
        
        public void OnHoverEnter(IViewItem incomingItem)
        {
            if (_isLockedInCell || _isAnimating) return;
            
            if (incomingItem == null || incomingItem.Data == null || this.Data == null) return;

            if (ReferenceEquals(incomingItem, this)) return;

            if (_mergeValidator.CanMerge(incomingItem.Data, this.Data))
            {
                _visualizer.SetMergeHintEffect(true);
            }
        }

        public void OnHoverExit(IViewItem incomingItem)
        {
            _visualizer.SetMergeHintEffect(false);
        }

        public void OnSpawned()
        {
            _isAnimating = false;
            _isLockedInCell = false;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            _visualizer.ResetSortingOrder();
            _visualizer.SetMergeHintEffect(false);
        }

        public void OnDespawned()
        {
            _visualizer.SetMergeHintEffect(false);
            
            _movementTween?.Kill(); 
            transform.DOKill(); 
            
            if (Data is SpawnerItemData itemData)
            {
                itemData.OnCooldownStateChanged -= _handleCooldownState;
            }

            Data = null;
            _dropReceiver = null;
            _interactRequestReceiver = null;
            _mergeValidator = null;
        }
    }
}