using System;
using Core.SaveSystem;
using Core.Services;

namespace Core.GridSystem
{
    public class SpawnerItemData : ItemData, ITimeTrackable
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
        
        public override ItemSaveData GetSaveData()
        {
            ItemSaveData data = base.GetSaveData();
            data.CurrentCapacity = this.CurrentCapacity;
            data.CooldownEndTimeTicks = this.CooldownEndTimeTicks;
            return data;
        }
    }
}