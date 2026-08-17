using UnityEngine;

namespace Data.UI
{
    [CreateAssetMenu(fileName = "FloatingTextConfig", menuName = "Configs/UI/Floating Text Config")]
    public class FloatingTextConfigSO : ScriptableObject
    {
        [Header("Movement")]
        public float Duration = 1.2f;
        public float RiseDistance = 100f;
        public AnimationCurve MoveYCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Fading")]
        public AnimationCurve AlphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("Scaling")]
        public bool UseScalePulse = true;
        public Vector2 RandomXOffsetRange = new Vector2(-30f, 30f);
        public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        
        [Header("Style")]
        public Color DefaultColor = Color.white;
        public Color AwesomeColor = Color.white;
    }
}