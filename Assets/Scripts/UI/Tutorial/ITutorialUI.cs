using System;
using UnityEngine;

namespace UI.Tutorial
{
    public interface ITutorialUI
    {
        event Action OnNextButtonClicked;
        void HighlightGridCells(Vector3 worldPos1, Vector3 worldPos2, float cellSize);
        void PlayHandAnimation(Vector3 startWorldPos, Vector3 endWorldPos);
        void HighlightOrderCompleteButton(RectTransform targetRect);
        void HighlightBackpackButton();
        void HighlightBackToMenuButton();
        void ShowCoreLoopPanel(Action onComplete);
        void SetMentorText(string text);
        void ClearHighlights();
    }
}