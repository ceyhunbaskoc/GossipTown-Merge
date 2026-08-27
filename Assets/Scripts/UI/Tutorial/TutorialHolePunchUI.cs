using UnityEngine;
using UnityEngine.UI;

namespace UI.Tutorial
{
    [RequireComponent(typeof(Image))]
    public class TutorialHolePunchUI : MonoBehaviour, ICanvasRaycastFilter
    {
        [SerializeField] private RectTransform _targetHole;

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (_targetHole == null) return true; 

            bool isInsideHole = RectTransformUtility.RectangleContainsScreenPoint(_targetHole, screenPoint, eventCamera);

            return !isInsideHole;
        }

        public void SetTargetHole(RectTransform target)
        {
            _targetHole = target;
        }
    }
}