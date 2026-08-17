namespace Core.Services
{
    public interface IInputLockService
    {
        bool IsLocked { get; }
        void AddLock();
        void RemoveLock();
        void ClearAllLocks();
    }
}