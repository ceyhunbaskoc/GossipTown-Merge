using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    public class IdleMonitorService : IDisposable
    {
        public event Action OnPlayerIdle;
        
        private readonly float _idleThresholdSeconds;
        private CancellationTokenSource _cancellationTokenSource;
        private float _lastInteractionTime;
        private bool _isIdleEventFired;

        public IdleMonitorService(float idleThresholdSeconds = 5f)
        {
            _idleThresholdSeconds = idleThresholdSeconds;
            _cancellationTokenSource = new CancellationTokenSource();
            ResetTimer();
            _ = MonitorIdleStateAsync(_cancellationTokenSource.Token);
        }
        public void ResetTimer()
        {
            _lastInteractionTime = Time.time;
            _isIdleEventFired = false;
        }

        private async Task MonitorIdleStateAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(250, token);

                    if (!_isIdleEventFired && Time.time - _lastInteractionTime >= _idleThresholdSeconds)
                    {
                        _isIdleEventFired = true;
                        OnPlayerIdle?.Invoke();
                    }
                }
            }
            catch (TaskCanceledException) {}
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}