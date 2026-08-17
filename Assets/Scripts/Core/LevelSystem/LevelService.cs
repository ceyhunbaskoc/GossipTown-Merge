using System;
using Core.SaveSystem;
using Data.Level;

namespace Core.LevelSystem
{
    public class LevelService : IReadOnlyLevel, ILevelModifier
    {
        public int CurrentExperience { get; private set; }
        public int CurrentLevel { get; private set; }
        public event Action<int> OnLevelChanged;
        public event Action<int, int> OnExperienceChanged;

        private readonly LevelProgressionSettingsSO _levelProgressionSettings;
        
        private const int INITIAL_LEVEL = 1;
        private const int INITIAL_EXPERIENCE = 0;
        
        public LevelService(LevelProgressionSettingsSO levelProgressionSettings, LevelSaveData levelSaveData)
        {
            _levelProgressionSettings = levelProgressionSettings;
            if(levelSaveData == null)
            {
                CurrentExperience = INITIAL_EXPERIENCE;
                CurrentLevel = INITIAL_LEVEL;
                return;
            }
            CurrentExperience = levelSaveData.CurrentExperience;
            CurrentLevel = levelSaveData.CurrentLevel;
        }
        
        public bool TryAddExperience(int amount)
        {
            if (amount <= 0 || CurrentLevel >= _levelProgressionSettings.MaxLevel) 
            {
                return false;
            }

            CurrentExperience += amount;

            int requiredExpForNextLevel = _levelProgressionSettings.GetRequiredExperienceForLevel(CurrentLevel + 1);

            while (CurrentExperience >= requiredExpForNextLevel && CurrentLevel < _levelProgressionSettings.MaxLevel)
            {
                CurrentExperience -= requiredExpForNextLevel;
                CurrentLevel++;
                OnLevelChanged?.Invoke(CurrentLevel);

                if (CurrentLevel >= _levelProgressionSettings.MaxLevel)
                {
                    CurrentExperience = 0;
                    requiredExpForNextLevel = 0;
                    break;
                }
                requiredExpForNextLevel = _levelProgressionSettings.GetRequiredExperienceForLevel(CurrentLevel + 1);
            }
    
            OnExperienceChanged?.Invoke(CurrentExperience, requiredExpForNextLevel);

            return true;
        }
        
        public LevelSaveData GetSaveData()
        {
            return new LevelSaveData
            {
                CurrentExperience = CurrentExperience,
                CurrentLevel = CurrentLevel
            };
        }
    }
}