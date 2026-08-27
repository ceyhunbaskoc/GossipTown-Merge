using Core;
using UnityEngine;
using Core.GridSystem;
using Core.Views;

namespace UI.Hints
{
    public class HintVisualOrchestrator : MonoBehaviour
    {
        private ItemVisualizer _item1Visualizer;
        private ItemVisualizer _item2Visualizer;

        public void PlayHintAnimation(IViewItem item1, IViewItem item2)
        {
            StopHintAnimation();

            if (item1 is MonoBehaviour v1 && v1.TryGetComponent(out DraggableItem drag1))
            {
                _item1Visualizer = drag1.Visualizer;
                _item1Visualizer?.PlayHintPulse();
            }

            if (item2 is MonoBehaviour v2 && v2.TryGetComponent(out DraggableItem drag2))
            {
                _item2Visualizer = drag2.Visualizer;
                _item2Visualizer?.PlayHintPulse();
            }
        }

        public void StopHintAnimation()
        {
            if (_item1Visualizer != null) _item1Visualizer.StopHintPulse();
            if (_item2Visualizer != null) _item2Visualizer.StopHintPulse();

            _item1Visualizer = null;
            _item2Visualizer = null;
        }
    }
}