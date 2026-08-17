using System.Collections.Generic;
using Core.Factories;
using Core.GridSystem;
using Core.Views;
using DG.Tweening;
using UnityEngine;

namespace Core.Orchestration
{
    public class OrderFulfillmentOrchestrator : MonoBehaviour
    {
        private MergeItemFactory _mergeItemFactory;
        
        public void Initialize(MergeItemFactory mergeItemFactory)
        {
            _mergeItemFactory = mergeItemFactory;
        }
        public void PlayFulfillmentSequence(List<IViewItem> items, Transform targetCardUI, System.Action onSequenceComplete)
        {
            if (items == null || items.Count == 0 || targetCardUI == null)
            {
                onSequenceComplete?.Invoke();
                return;
            }
            Sequence fulfillSequence = DOTween.Sequence();
            Vector3 targetWorldPos = targetCardUI.position;
            targetWorldPos.z = 0f;

            foreach (var item in items)
            {
                if (item is MonoBehaviour viewBehaviour)
                {
                    Transform itemTransform = viewBehaviour.transform;
                    
                    fulfillSequence.Insert(0f, itemTransform.DOMove(targetWorldPos, 0.5f).SetEase(Ease.InBack));
                    fulfillSequence.Insert(0f, itemTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
                }
            }

            fulfillSequence.AppendCallback(() => 
            {
                foreach (var item in items)
                {
                    _mergeItemFactory.RecycleItem(item);
                }
            });

            fulfillSequence.OnComplete(() => 
            {
                onSequenceComplete?.Invoke();
            });
        }
    }
    
}