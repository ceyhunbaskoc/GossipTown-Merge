using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    public class TimeManager : ITimeManager, IDisposable
    {
        private readonly List<ITimeTrackable> _trackedTimers = new List<ITimeTrackable>();
        
        private readonly CancellationTokenSource _cancellationTokenSource;

        public long CurrentTimeTicks => DateTime.UtcNow.Ticks;

        public TimeManager()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            _ = CooldownTickerRoutineAsync(_cancellationTokenSource.Token);
        }

        public void RegisterTimer(ITimeTrackable timer)
        {
            if (!_trackedTimers.Contains(timer))
            {
                _trackedTimers.Add(timer);
            }
        }

        private async Task CooldownTickerRoutineAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                    
                    long currentTicks = CurrentTimeTicks;

                    for (int i = _trackedTimers.Count - 1; i >= 0; i--)
                    {
                        ITimeTrackable timer = _trackedTimers[i];

                        if (currentTicks >= timer.TargetTimeTicks)
                        {
                            timer.OnTimeCompleted();
                            _trackedTimers.RemoveAt(i);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TimeManager] Zamanlayıcı döngüsünde kritik hata: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            
            _cancellationTokenSource.Dispose();
            _trackedTimers.Clear();
        }
    }
}