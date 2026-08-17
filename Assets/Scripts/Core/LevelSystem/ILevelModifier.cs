namespace Core.LevelSystem
{
    public interface ILevelModifier
    {
        bool TryAddExperience(int amount);
    }
}