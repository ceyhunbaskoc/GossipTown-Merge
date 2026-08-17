using UnityEngine;
using System;
using Data.Roadmap;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace UI.Map
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BuildingSlotView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        [Header("World Space References")]
        [SerializeField] private SpriteRenderer _buildingSpriteRenderer;
        [SerializeField] private GameObject _lockedOverlay; 
        [SerializeField] private GameObject _upgradeReadyFx; 

        [field: SerializeField] public MapNodeDefinitionSO NodeDefinition { get; private set; }

        public event Action<string> OnSlotClicked;

        private int _pendingLevel;
        private bool _pendingUnlockedState;
        private bool _pendingUpgradeState;
        
        private Vector2 _pointerDownPosition;
        private const float DragThreshold = 15f;
        public void UpdateVisualState(int currentLevel, bool canUpgrade, bool isUnlocked, bool animate = false)
        {
            if (animate)
            {
                _pendingLevel = currentLevel;
                _pendingUpgradeState = canUpgrade;
                _pendingUnlockedState = isUnlocked;
                PlayUpgradeAnimation();
            }
            else
            {
                ApplyVisuals(currentLevel, canUpgrade, isUnlocked);
            }
        }

        private void ApplyVisuals(int level, bool canUpgrade, bool isUnlocked)
        {
            if (!isUnlocked)
            {
                _lockedOverlay.SetActive(true);
                _buildingSpriteRenderer.color = Color.gray; 
                _buildingSpriteRenderer.sprite = NodeDefinition.Building.GetLevelData(1)?.LevelSprite; 
            }
            else if (level == 0)
            {
                _lockedOverlay.SetActive(false);
                _buildingSpriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                _buildingSpriteRenderer.sprite = NodeDefinition.Building.GetLevelData(1)?.LevelSprite;
            }
            else
            {
                _lockedOverlay.SetActive(false);
                _buildingSpriteRenderer.color = Color.white;
        
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

            transform.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.1f).SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    ApplyVisuals(_pendingLevel, _pendingUpgradeState, _pendingUnlockedState);
                    transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.15f).SetEase(Ease.OutBack)
                        .OnComplete(() => {
                            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutElastic);
                        });
                });
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownPosition = eventData.position;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            float dragDistance = Vector2.Distance(_pointerDownPosition, eventData.position);
            if (dragDistance > DragThreshold)
            {
                return;
            }

            OnSlotClicked?.Invoke(NodeDefinition.NodeId);
        }
    }
}