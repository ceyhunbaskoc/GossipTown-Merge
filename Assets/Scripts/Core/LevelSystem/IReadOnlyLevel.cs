using System;

namespace Core.LevelSystem
{
    public interface IReadOnlyLevel
    {
        int CurrentExperience { get; }
        int CurrentLevel { get;  }

        event Action<int> OnLevelChanged;
        event Action<int, int> OnExperienceChanged;
    }
}