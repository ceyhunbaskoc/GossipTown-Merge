using System;

namespace Core.Services
{
    public class MidnightTimer : ITimeTrackable
    {
        public long TargetTimeTicks { get; private set; }
        private readonly Action _onMidnightReached;

        public MidnightTimer(long targetTicks, Action onMidnightReached)
        {
            TargetTimeTicks = targetTicks;
            _onMidnightReached = onMidnightReached;
        }

        public void OnTimeCompleted()
        {
            _onMidnightReached?.Invoke();
        }
    }
}