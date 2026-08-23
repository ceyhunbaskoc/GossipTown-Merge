using UnityEngine;
using System;
using Data.Roadmap;
using DG.Tweening;
using UnityEngine.EventSystems;
using Core.PoolSystem;

namespace UI.Map
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BuildingSlotView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        [Header("World Space References")]
        [SerializeField] private SpriteRenderer _buildingSpriteRenderer;
        [SerializeField] private GameObject _lockedOverlay; 
        [SerializeField] private GameObject _upgradeReadyFx; 
        
        [Header("Particle Types")]
        [SerializeField] private PoolObjectType _dustParticleType;
        [SerializeField] private PoolObjectType _confettiParticleType;

        [field: SerializeField] public MapNodeDefinitionSO NodeDefinition { get; private set; }

        public event Action<string> OnSlotClicked;
        public event Action<string> OnUpgradeAnimationCompleted;

        private IObjectPool _objectPool;
        private int _pendingLevel;
        private bool _pendingUnlockedState;
        private bool _pendingUpgradeState;
        private Sprite _unBuildSprite;
        
        private Vector3 _originalScale;
        private bool _isAnimating; 
        
        private Vector2 _pointerDownPosition;
        private const float DragThreshold = 15f;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void Initialize(IObjectPool objectPool, Sprite unBuildSprite)
        {
            _objectPool = objectPool;
            _unBuildSprite = unBuildSprite;
        }

        public void UpdateVisualState(int currentLevel, bool canUpgrade, bool isUnlocked, bool animate = false)
        {
            if (animate)
            {
                _isAnimating = true;
                _pendingLevel = currentLevel;
                _pendingUpgradeState = canUpgrade;
                _pendingUnlockedState = isUnlocked;
                PlayUpgradeAnimation();
            }
            else
            {
                if (!_isAnimating)
                {
                    ApplyVisuals(currentLevel, canUpgrade, isUnlocked);
                }
                else
                {
                    _pendingUpgradeState = canUpgrade;
                }
            }
        }

        private void ApplyVisuals(int level, bool canUpgrade, bool isUnlocked)
        {
            if (!isUnlocked)
            {
                _lockedOverlay.SetActive(true);
                _buildingSpriteRenderer.sprite = _unBuildSprite; 
            }
            else if (level == 0)
            {
                _lockedOverlay.SetActive(false);
                _buildingSpriteRenderer.sprite = _unBuildSprite;
            }
            else
            {
                _lockedOverlay.SetActive(false);
        
                var currentLevelData = NodeDefinition.Building.GetLevelData(level);
                if (currentLevelData != null && currentLevelData.LevelSprite != null)
                {
                    _buildingSpriteRenderer.sprite = currentLevelData.LevelSprite;
                }
            }

            _upgradeReadyFx.SetActive(canUpgrade);
        }

        private void PlayUpgradeAnimation()
        {
            transform.DOKill();
            transform.localScale = _originalScale;
            _upgradeReadyFx.SetActive(false);

            Sequence upgradeSeq = DOTween.Sequence();

            int constructionSteps = 3;
            float stepDuration = 0.2f;

            for (int i = 0; i < constructionSteps; i++)
            {
                int stepIndex = i; 
                
                upgradeSeq.AppendCallback(() => _spawnParticle(_dustParticleType));
                
                float shakeStrength = 0.05f + (stepIndex * 0.05f);
                upgradeSeq.Append(transform.DOShakePosition(stepDuration, new Vector3(shakeStrength, 0, 0), 15, 90, false, true));
                
                upgradeSeq.AppendInterval(0.1f);
            }

            Vector3 stretchScale = new Vector3(_originalScale.x * 0.85f, _originalScale.y * 1.35f, _originalScale.z);
            upgradeSeq.Append(transform.DOScale(stretchScale, 0.2f).SetEase(Ease.OutQuad))
                .AppendCallback(() => 
                {
                    ApplyVisuals(_pendingLevel, _pendingUpgradeState, _pendingUnlockedState);
                });

            Vector3 squashScale = new Vector3(_originalScale.x * 1.25f, _originalScale.y * 0.75f, _originalScale.z);
            upgradeSeq.Append(transform.DOScale(squashScale, 0.15f).SetEase(Ease.OutBack))
                .Append(transform.DOScale(_originalScale, 0.25f).SetEase(Ease.OutElastic));

            upgradeSeq.AppendCallback(() => _spawnParticle(_confettiParticleType));
            upgradeSeq.AppendInterval(1.0f);

            upgradeSeq.OnComplete(() => 
            {
                _isAnimating = false;
                OnUpgradeAnimationCompleted?.Invoke(NodeDefinition.NodeId);
            });
        }
        
        private void _spawnParticle(PoolObjectType particleType)
        {
            if (_objectPool != null)
            {
                _objectPool.Spawn(particleType, transform.position, Quaternion.identity);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownPosition = eventData.position;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isAnimating) return; 
            
            float dragDistance = Vector2.Distance(_pointerDownPosition, eventData.position);
            if (dragDistance > DragThreshold) return;

            OnSlotClicked?.Invoke(NodeDefinition.NodeId);
        }
    }
}