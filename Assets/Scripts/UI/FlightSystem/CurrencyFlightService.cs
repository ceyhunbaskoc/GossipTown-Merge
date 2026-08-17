using System;
using Core.PoolSystem;
using UnityEngine;
using DG.Tweening;
using Data.Quests;
using Data.Reward;

namespace UI.FlightSystem
{
    public class CurrencyFlightService : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Canvas _flightCanvas; 
        private RectTransform _flightCanvasRect;
        
        [Header("Settings")]
        [SerializeField] private float _scatterRadius = 100f;
        [SerializeField] private float _flightDuration = 0.6f;

        private IHUDTargetProvider _targetProvider;
        private IObjectPool _poolService;
        
        private GlobalRewardIconDatabaseSO _iconDatabase;

        public void Initialize(IHUDTargetProvider targetProvider, IObjectPool poolService, GlobalRewardIconDatabaseSO iconDatabase)
        {
            _targetProvider = targetProvider;
            _poolService = poolService;
            _iconDatabase = iconDatabase;
            
            if (_flightCanvas != null)
            {
                _flightCanvasRect = _flightCanvas.GetComponent<RectTransform>();
            }
        }

        public void PlayFlightAnimation(RewardCategory category, int amount, Vector3 startWorldPosition, Action onComplete = null)
        {
            Sprite iconSprite = _iconDatabase.GetIconForCategory(category);
            if (iconSprite == null)
            {
                return;
            }

            Vector3 targetWorldPos = _targetProvider.GetTargetScreenPosition(category);
            
            Camera mainCamera = Camera.main; 
            Camera canvasCamera = _flightCanvas.worldCamera;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, startWorldPosition);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _flightCanvasRect,
                screenPos, 
                canvasCamera,
                out Vector2 localStartPos);

            int visualCount = Mathf.Clamp(amount, 3, 8);
            int completedAnimations = 0;

            for (int i = 0; i < visualCount; i++)
            {
                FlightItemView flightItem = _poolService.Spawn<FlightItemView>(PoolObjectType.FlightItem, Vector3.zero, Quaternion.identity, _flightCanvasRect);

                flightItem.SetVisual(iconSprite);
                flightItem.RectTransform.localPosition = localStartPos;
                flightItem.RectTransform.localScale = Vector3.zero;

                Vector2 scatterPos = localStartPos + (UnityEngine.Random.insideUnitCircle * _scatterRadius);
                
                Sequence flightSequence = DOTween.Sequence();
                flightSequence.SetLink(flightItem.gameObject);
                
                flightSequence.Append(flightItem.RectTransform.DOLocalMove(scatterPos, 0.3f).SetEase(Ease.OutQuad));
                flightSequence.Join(flightItem.RectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
                flightSequence.AppendInterval(UnityEngine.Random.Range(0f, 0.15f));
                flightSequence.Append(flightItem.RectTransform.DOMove(targetWorldPos, _flightDuration).SetEase(Ease.InBack));
                
                flightSequence.OnComplete(() =>
                {
                    _poolService.Despawn<FlightItemView>(PoolObjectType.FlightItem, flightItem);
                    completedAnimations++;
                    
                    if (completedAnimations == visualCount)
                    {
                        onComplete?.Invoke();
                    }
                });
            }
        }
    }
}