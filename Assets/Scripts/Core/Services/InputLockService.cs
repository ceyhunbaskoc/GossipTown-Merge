using System;

namespace Core.Services
{
    public class InputLockService : IInputLockService
    {
        private int _gameplayLockCount;
        private int _uiLockCount;

        public event Action<bool> OnLockStateChanged;
        public event Action<bool> OnUILockStateChanged;

        public bool IsLocked => _gameplayLockCount > 0;
        public bool IsUILocked => _uiLockCount > 0;

        #region Gameplay (Board/Grid) Locks

        public void AddLock()
        {
            _gameplayLockCount++;
            if (_gameplayLockCount == 1)
            {
                OnLockStateChanged?.Invoke(true);
            }
        }

        public void RemoveLock()
        {
            if (_gameplayLockCount > 0)
            {
                _gameplayLockCount--;
                if (_gameplayLockCount == 0)
                {
                    OnLockStateChanged?.Invoke(false);
                }
            }
        }

        #endregion

        #region UI (Panels/Buttons) Locks

        public void AddUILock()
        {
            _uiLockCount++;
            if (_uiLockCount == 1)
            {
                OnUILockStateChanged?.Invoke(true);
            }
        }

        public void RemoveUILock()
        {
            if (_uiLockCount > 0)
            {
                _uiLockCount--;
                if (_uiLockCount == 0)
                {
                    OnUILockStateChanged?.Invoke(false);
                }
            }
        }

        #endregion

        public void ClearAllLocks()
        {
            if (_gameplayLockCount > 0)
            {
                _gameplayLockCount = 0;
                OnLockStateChanged?.Invoke(false);
            }

            if (_uiLockCount > 0)
            {
                _uiLockCount = 0;
                OnUILockStateChanged?.Invoke(false);
            }
        }
    }
}