using System;
using Core.GridSystem;
using DG.Tweening;
using UnityEngine;

namespace UI.Collectible
{
    public class CollectibleCollectOrchestrator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _anticipationDuration = 0.15f;
        [SerializeField] private float _anticipationScaleMultiplier = 1.25f;
        [SerializeField] private Ease _anticipationEase = Ease.OutQuad;

        [SerializeField] private float _popDuration = 0.2f;
        [SerializeField] private Ease _popEase = Ease.InBack;
        
        public void PlayCollectSequence(IViewItem collectibleView, Action onSequenceComplete)
        {
            if (collectibleView == null || !(collectibleView is Component collectibleComponent))
            {
                onSequenceComplete?.Invoke();
                return;
            }

            Transform collectibleTransform = collectibleComponent.transform;
            
            Vector3 originalScale = collectibleTransform.localScale;
            Vector3 targetScale = originalScale * _anticipationScaleMultiplier;

            collectibleTransform.DOKill();

            Sequence collectSequence = DOTween.Sequence();

            collectSequence.Append(collectibleTransform.DOScale(targetScale, _anticipationDuration)
                .SetEase(_anticipationEase));

            collectSequence.Append(collectibleTransform.DOScale(Vector3.zero, _popDuration)
                .SetEase(_popEase));

            collectSequence.OnComplete(() =>
            {
                collectibleTransform.localScale = originalScale;
                onSequenceComplete?.Invoke();
            });
        }
    }
}