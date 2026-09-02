namespace Core.Haptics
{
    public enum HapticType
    {
        Selection,
        LightImpact,
        MediumImpact,
        HeavyImpact,
        Success,
        Warning,
        SoftImpact
    }

    public interface IHapticService
    {
        void Play(HapticType type);
    }
}