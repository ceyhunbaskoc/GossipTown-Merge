using System;
using UnityEngine;

namespace UI.Tutorial
{
    public interface ITutorialUI
    {
        void HighlightGridCells(Vector3 worldPos1, Vector3 worldPos2, float cellSize);
        void PlayHandAnimation(Vector3 startWorldPos, Vector3 endWorldPos);
        void HighlightOrderCompleteButton(RectTransform targetRect);
        void ShowCoreLoopPanel(Action onComplete);
        void ClearHighlights();
    }
}