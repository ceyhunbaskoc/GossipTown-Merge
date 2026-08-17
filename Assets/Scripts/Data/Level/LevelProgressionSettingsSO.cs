using UnityEngine;

namespace Data.Level
{
    [CreateAssetMenu(fileName = "LevelProgressionSettings", menuName = "GameData/Progression/LevelProgressionSettings")]
    public class LevelProgressionSettingsSO : ScriptableObject
    {
        [Header("Experience Curve Setup")]
        [Tooltip("X Axis: Level, Y Axis: Required Experience Points")]
        [SerializeField] private AnimationCurve _experienceCurve;

        [Header("Progression Constraints")]
        [SerializeField] private int _maxLevel = 200;

        public int MaxLevel => _maxLevel;
        
        public int GetRequiredExperienceForLevel(int level)
        {
            if (level <= 1) return 0;
            int clampedLevel = Mathf.Clamp(level, 1, _maxLevel);
            float rawExperience = _experienceCurve.Evaluate(clampedLevel);
            return Mathf.RoundToInt(rawExperience);
        }
    }
}