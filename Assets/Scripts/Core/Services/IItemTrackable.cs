namespace Core.Services
{
    public interface ITimeTrackable
    {
        long TargetTimeTicks { get; }
        void OnTimeCompleted();
    }
}