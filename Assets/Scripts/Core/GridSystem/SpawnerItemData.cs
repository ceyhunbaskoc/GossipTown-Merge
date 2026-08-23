using System;
using Core.SaveSystem;
using Core.Services;
using UnityEngine;

namespace Core.GridSystem
{
    public class SpawnerItemData : ItemData, ITimeTrackable, ICustomSaveableItem
    {
        public int CurrentCapacity { get; set; }
        public int MaxCapacity { get; private set; }
        public long CooldownEndTimeTicks { get; set; }
        
        public long TargetTimeTicks => CooldownEndTimeTicks;

        private bool _isInCooldown;
        public bool IsInCooldown 
        { 
            get => _isInCooldown; 
            private set
            {
                _isInCooldown = value;
                OnCooldownStateChanged?.Invoke(_isInCooldown); 
            }
        }
        
        public event Action<bool> OnCooldownStateChanged;

        public SpawnerItemData(string id, int level, int maxCapacity) : base(id, level)
        {
            MaxCapacity = maxCapacity;
            CurrentCapacity = maxCapacity;
        }
        
        public string GetCustomStateJson()
        {
            SpawnerSaveState state = new SpawnerSaveState
            {
                CurrentCapacity = this.CurrentCapacity,
                CooldownEndTimeTicks = this.CooldownEndTimeTicks
            };
            return JsonUtility.ToJson(state);
        }

        public void LoadCustomStateFromJson(string json)
        {
            SpawnerSaveState state = JsonUtility.FromJson<SpawnerSaveState>(json);
            if (state == null) return;

            CurrentCapacity = state.CurrentCapacity;
            
            if (state.CooldownEndTimeTicks > 0)
            {
                if (DateTime.UtcNow.Ticks >= state.CooldownEndTimeTicks)
                {
                    WakeUp();
                }
                else
                {
                    EnterCooldown(state.CooldownEndTimeTicks);
                }
            }
        }
        
        public void InitializeSpawnerCapacity(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            CurrentCapacity = maxCapacity;
        }

        public void EnterCooldown(long endTimeTicks)
        {
            CooldownEndTimeTicks = endTimeTicks;
            IsInCooldown = true;
        }

        public void WakeUp()
        {
            CurrentCapacity = MaxCapacity;
            IsInCooldown = false;
        }

        public void OnTimeCompleted()
        {
            WakeUp();
        }
    }
}