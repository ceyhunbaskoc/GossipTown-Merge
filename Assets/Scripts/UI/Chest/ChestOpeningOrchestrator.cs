using System;
using Core.GridSystem;
using DG.Tweening;
using UnityEngine;

namespace UI.Chest
{
    public class ChestOpeningOrchestrator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _breatheDuration = 0.25f;
        [SerializeField] private float _breatheScaleMultiplier = 1.15f;
        [SerializeField] private int _breatheLoops = 4;

        [SerializeField] private float _burstDuration = 0.2f;
        [SerializeField] private float _burstScaleMultiplier = 1.4f;

        [SerializeField] private float _popDuration = 0.15f;

        [Header("Ease Curves")]
        [SerializeField] private Ease _breatheEase = Ease.InOutSine;
        [SerializeField] private Ease _burstEase = Ease.OutBack;
        [SerializeField] private Ease _popEase = Ease.InBack;

        public void PlayOpeningSequence(IViewItem chestView, Action onLidOpened, Action onSequenceComplete)
        {
            if (chestView == null || !(chestView is Component chestComponent))
            {
                Debug.LogError("[ChestOpeningOrchestrator] PlayOpeningSequence called but View is null! Fallback triggered.");
                onLidOpened?.Invoke();
                onSequenceComplete?.Invoke();
                return;
            }

            Transform chestTransform = chestComponent.transform;
            Vector3 originalScale = chestTransform.localScale;
            
            Vector3 breatheScale = originalScale * _breatheScaleMultiplier;
            Vector3 burstScale = originalScale * _burstScaleMultiplier;

            chestTransform.DOKill();

            Sequence openingSequence = DOTween.Sequence();

            openingSequence.Append(chestTransform.DOScale(breatheScale, _breatheDuration)
                .SetLoops(_breatheLoops, LoopType.Yoyo)
                .SetEase(_breatheEase));

            openingSequence.Append(chestTransform.DOScale(burstScale, _burstDuration)
                .SetEase(_burstEase));

            openingSequence.AppendCallback(() => onLidOpened?.Invoke());

            openingSequence.Append(chestTransform.DOScale(Vector3.zero, _popDuration)
                .SetEase(_popEase));

            openingSequence.OnComplete(() =>
            {
                chestTransform.localScale = originalScale;
                onSequenceComplete?.Invoke();
            });
        }
    }
}