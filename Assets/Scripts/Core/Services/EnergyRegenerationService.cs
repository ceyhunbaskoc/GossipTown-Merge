using System;
using Core.Economy;
using UnityEngine;

namespace Core.Services
{
    public class EnergyRegenerationService : ITimeTrackable, IDisposable
    {
        private readonly IEconomyModifier _economyModifier;
        private readonly IReadOnlyEconomyModel _economyModel;
        private readonly ITimeManager _timeManager;
        
        private readonly int _regenIntervalSeconds;
        private readonly long _regenIntervalTicks;
        
        public long TargetTimeTicks { get; private set; }
        public bool IsTimerRunning { get; private set; }

        public EnergyRegenerationService(
            IEconomyModifier economyModifier, 
            IReadOnlyEconomyModel economyModel, 
            ITimeManager timeManager,
            int regenIntervalSeconds = 120,
            long savedTargetTimeTicks = 0)
        {
            _economyModifier = economyModifier;
            _economyModel = economyModel;
            _timeManager = timeManager;
            _regenIntervalSeconds = regenIntervalSeconds;
            _regenIntervalTicks = TimeSpan.FromSeconds(regenIntervalSeconds).Ticks;

            _economyModel.OnEnergySpend += HandleEnergySpent;

            ProcessOfflineProgression(savedTargetTimeTicks);
        }

        private void ProcessOfflineProgression(long savedTargetTicks)
        {
            if (!_economyModel.CanEnergyRegeneration)
            {
                StopTimer();
                return;
            }

            if (savedTargetTicks <= 0)
            {
                StartNewCycle();
                return;
            }

            long currentTicks = _timeManager.CurrentTimeTicks;

            if (currentTicks < savedTargetTicks)
            {
                TargetTimeTicks = savedTargetTicks;
                RegisterTimer();
                return;
            }

            long passedTicks = currentTicks - savedTargetTicks;
            int missedCycles = 1 + (int)(passedTicks / _regenIntervalTicks);

            int energyToAdd = Mathf.Min(missedCycles, _economyModel.EnergyRegenerationTrigger - _economyModel.Energy);
            
            if (energyToAdd > 0)
            {
                _economyModifier.AddEnergy(energyToAdd);
                Debug.Log($"[EnergyRegen] Offline kalınan sürede {energyToAdd} enerji kazanıldı.");
            }

            if (!_economyModel.CanEnergyRegeneration)
            {
                StopTimer();
            }
            else
            {
                long leftoverTicks = passedTicks % _regenIntervalTicks;
                TargetTimeTicks = currentTicks + (_regenIntervalTicks - leftoverTicks);
                RegisterTimer();
            }
        }

        private void HandleEnergySpent(int currentEnergy)
        {
            if (!IsTimerRunning && _economyModel.CanEnergyRegeneration)
            {
                StartNewCycle();
            }
        }

        private void StartNewCycle()
        {
            TargetTimeTicks = _timeManager.CurrentTimeTicks + _regenIntervalTicks;
            RegisterTimer();
        }

        private void RegisterTimer()
        {
            IsTimerRunning = true;
            _timeManager.RegisterTimer(this);
        }

        private void StopTimer()
        {
            IsTimerRunning = false;
            TargetTimeTicks = 0;
        }

        public void OnTimeCompleted()
        {
            IsTimerRunning = false;

            if (_economyModel.CanEnergyRegeneration)
            {
                _economyModifier.AddEnergy(1);
                
                if (_economyModel.CanEnergyRegeneration)
                {
                    StartNewCycle();
                }
                else
                {
                    StopTimer();
                }
            }
        }

        public int GetRemainingSeconds()
        {
            if (!IsTimerRunning) return 0;
            long remainingTicks = TargetTimeTicks - _timeManager.CurrentTimeTicks;
            return Mathf.Max(0, (int)TimeSpan.FromTicks(remainingTicks).TotalSeconds);
        }

        public void Dispose()
        {
            _economyModel.OnEnergySpend -= HandleEnergySpent;
        }
    }
}