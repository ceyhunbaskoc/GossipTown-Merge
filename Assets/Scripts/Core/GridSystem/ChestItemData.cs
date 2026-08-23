using System;
using Core.SaveSystem;
using Core.Services;
using UnityEngine;

namespace Core.GridSystem
{
    public enum ChestState { Locked, Unlocking, ReadyToOpen }

    public class ChestItemData : ItemData, ITimeTrackable, ICustomSaveableItem 
    {
        public ChestState CurrentState { get; private set; }
        public long UnlockTargetTimeTicks { get; private set; }
        public long TargetTimeTicks => UnlockTargetTimeTicks;
        public event Action<ChestState> OnStateChanged;

        public ChestItemData(string id, int level) : base(id, level)
        {
            CurrentState = ChestState.Locked;
        }

        public string GetCustomStateJson()
        {
            ChestSaveState state = new ChestSaveState
            {
                CurrentState = (int)this.CurrentState,
                UnlockTargetTimeTicks = this.UnlockTargetTimeTicks
            };
            return JsonUtility.ToJson(state);
        }

        public void LoadCustomStateFromJson(string json)
        {
            ChestSaveState state = JsonUtility.FromJson<ChestSaveState>(json);
            if (state == null) return;

            CurrentState = (ChestState)state.CurrentState;
            UnlockTargetTimeTicks = state.UnlockTargetTimeTicks;

            if (CurrentState == ChestState.Unlocking && DateTime.UtcNow.Ticks >= UnlockTargetTimeTicks)
            {
                CompleteUnlocking();
            }
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