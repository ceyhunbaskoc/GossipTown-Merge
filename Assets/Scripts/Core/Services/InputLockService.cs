namespace Core.Services
{
    public class InputLockService : IInputLockService
    {
        private int _lockCount;
        public bool IsLocked => _lockCount > 0;

        public void AddLock()
        {
            _lockCount++;
        }

        public void RemoveLock()
        {
            _lockCount--;
            if (_lockCount < 0) _lockCount = 0;
        }

        public void ClearAllLocks()
        {
            _lockCount = 0;
        }
    }
}