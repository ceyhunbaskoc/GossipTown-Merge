using UnityEngine;
using DG.Tweening;

namespace Core.Views
{
    public class ItemVisualizer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private SpriteRenderer _visualRenderer; 
        [SerializeField] private SpriteRenderer _raycastProxyRenderer; 
        [SerializeField] private GameObject _notificationBadge; 
        
        [SerializeField] private int _dragSortingOrder = 100;
        [Header("Effects")]
        [SerializeField] private ParticleSystem _mergeHintParticle;
        
        public void SetMergeHintEffect(bool isActive)
        {
            if (_mergeHintParticle == null) return;

            if (isActive)
            {
                if (!_mergeHintParticle.isPlaying) _mergeHintParticle.Play();
            }
            else
            {
                _mergeHintParticle.Stop();
                _mergeHintParticle.Clear();
            }
        }
        
        private int _originalSortingOrder;
        private string _originalSortingLayer;
        
        private const string LAYER_DRAGGED_ITEM = "DraggedItem";
        
        private Vector3 _baseScale; 
        private Vector3 _pickedUpScale;
        
        private bool _isPickedUp = false;
        private Tween _hintTween;
        
        private void Awake()
        {
            if (_visualRenderer != null)
            {
                _originalSortingLayer = _visualRenderer.sortingLayerName;
                _originalSortingOrder = _visualRenderer.sortingOrder;
            }
            
            _baseScale = transform.localScale;
            _pickedUpScale = _baseScale * 1.2f;
        }

        private void OnDestroy() => transform.DOKill();

        public void SetVisual(Sprite icon)
        {
            if (_visualRenderer != null) _visualRenderer.sprite = icon;
        }
        
        public void SetBadgeActive(bool isActive)
        {
            if (_notificationBadge != null)
            {
                _notificationBadge.SetActive(isActive);
            }
        }
        
        public void SetColor(Color color) { if (_visualRenderer != null) _visualRenderer.color = color; }

        public void PlayPickUpAnimation()
        {
            _isPickedUp = true;
            transform.DOKill();
            transform.DOScale(_pickedUpScale, 0.2f).SetEase(Ease.OutBack);
        }

        public void PlayDropAnimation()
        {
            _isPickedUp = false;
            transform.DOKill();
            transform.DOScale(_baseScale, 0.2f).SetEase(Ease.InBack);
        }
        
        public void PlaySpawnAnimation()
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(_baseScale, 0.4f).SetEase(Ease.OutBounce);
        }

        public void PlayClickAnimation()
        {
            transform.DOKill();
            Sequence clickSequence = DOTween.Sequence();
            clickSequence.Append(transform.DOScale(_pickedUpScale, 0.2f).SetEase(Ease.OutBounce));
            clickSequence.Append(transform.DOScale(_baseScale, 0.4f).SetEase(Ease.OutBounce));
            clickSequence.SetTarget(transform); 
        }

        public void PlayClearAnimation()
        {
            transform.DOKill();
            transform.localScale = _baseScale;
            transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.OutBounce);
        }
        
        public void BringToFront()
        {
            SetSortingData(LAYER_DRAGGED_ITEM, _dragSortingOrder);
        }

        public void ResetSortingOrder()
        {
            SetSortingData(_originalSortingLayer, _originalSortingOrder);
        }
        private void SetSortingData(string layerName, int order)
        {
            if (_visualRenderer != null)
            {
                _visualRenderer.sortingLayerName = layerName;
                _visualRenderer.sortingOrder = order;
            }

            if (_raycastProxyRenderer != null)
            {
                _raycastProxyRenderer.sortingLayerName = layerName;
                _raycastProxyRenderer.sortingOrder = order;
            }
        }
        
        public void PlayHintPulse()
        {
            if (_isPickedUp) return;
            transform.DOKill();
            _hintTween = transform.DOScale(_baseScale * 1.15f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        public void StopHintPulse()
        {
            if (_hintTween != null && _hintTween.IsActive())
            {
                _hintTween.Kill();
                _hintTween = null;
                if (!_isPickedUp) transform.localScale = _baseScale;
            }
        }
    }
}