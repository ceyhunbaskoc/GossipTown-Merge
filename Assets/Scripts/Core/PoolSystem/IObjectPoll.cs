using UnityEngine;

namespace Core.PoolSystem
{
    public interface IObjectPool
    {
        T Spawn<T>(PoolObjectType type, Vector3 position, Quaternion rotation, float lifetime = 0f) where T : Component;
        T Spawn<T>(PoolObjectType type, Vector3 position, Quaternion rotation, Transform parent, float lifetime = 0f) where T : Component;
        GameObject Spawn(PoolObjectType type, Vector3 position, Quaternion rotation, Transform parent = null, float lifetime = 0f);
        T SpawnUI<T>(PoolObjectType type, Transform parent=null, float lifetime = 0f) where T : Component;

        void Despawn<T>(PoolObjectType type, T component) where T : Component;
        void Despawn(PoolObjectType type, GameObject obj);
    }
}