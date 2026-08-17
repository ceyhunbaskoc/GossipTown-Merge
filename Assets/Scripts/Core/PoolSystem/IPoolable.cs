namespace Core.PoolSystem
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}