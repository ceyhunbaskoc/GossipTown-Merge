using System;

namespace Core.Services
{
    public interface IInputLockService
    {
        bool IsLocked { get; }
        event Action<bool> OnLockStateChanged;
        void AddLock();
        void RemoveLock();

        bool IsUILocked { get; }
        event Action<bool> OnUILockStateChanged;
        void AddUILock();
        void RemoveUILock();

        void ClearAllLocks();
    }
}