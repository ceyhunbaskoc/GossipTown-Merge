namespace Core.Services
{
    public interface ITimeManager
    {
        long CurrentTimeTicks { get; }
        void RegisterTimer(ITimeTrackable timer);
    }
}