using System;
using Core.Services;

namespace Core.GridSystem
{
    public enum ChestState { Locked, Unlocking, ReadyToOpen }

    public class ChestItemData : ItemData, ITimeTrackable
    {
        public ChestState CurrentState { get; private set; }
        public long UnlockTargetTimeTicks { get; private set; }
        public long TargetTimeTicks => UnlockTargetTimeTicks;
        public event Action<ChestState> OnStateChanged;

        public ChestItemData(string id, int level) : base(id, level)
        {
            CurrentState = ChestState.Locked;
        }

        public void StartUnlocking(long targetTimeTicks)
        {
            UnlockTargetTimeTicks = targetTimeTicks;
            ChangeState(ChestState.Unlocking);
        }

        public void ForceComplete()
        {
            UnlockTargetTimeTicks = 0;
            ChangeState(ChestState.ReadyToOpen);
        }

        public void CompleteUnlocking()
        {
            ChangeState(ChestState.ReadyToOpen);
        }

        public void OnTimeCompleted()
        {
            if (CurrentState == ChestState.Unlocking)
            {
                CompleteUnlocking();
            }
        }

        private void ChangeState(ChestState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}