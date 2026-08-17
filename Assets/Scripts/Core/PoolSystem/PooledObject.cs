using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.PoolSystem
{
    public class PooledObject : MonoBehaviour
    {
        private IObjectPool<GameObject> _managedPool;
        private IPoolable[] _poolables;
        private Coroutine _lifetimeCoroutine;

        public void Initialize(IObjectPool<GameObject> pool)
        {
            _managedPool = pool;
            _poolables = GetComponentsInChildren<IPoolable>(true);
        }

        public void Activate(float lifetime = 0f)
        {
            foreach (var poolable in _poolables)
            {
                poolable.OnSpawned();
            }

            if (_lifetimeCoroutine != null) StopCoroutine(_lifetimeCoroutine);

            if (lifetime > 0f)
            {
                _lifetimeCoroutine = StartCoroutine(LifetimeRoutine(lifetime));
            }
        }

        private IEnumerator LifetimeRoutine(float time)
        {
            yield return new WaitForSeconds(time);
            ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (_lifetimeCoroutine != null) StopCoroutine(_lifetimeCoroutine);

            foreach (var poolable in _poolables)
            {
                poolable.OnDespawned();
            }

            if (_managedPool != null)
            {
                _managedPool.Release(gameObject);
            }
            else
            {
                Destroy(gameObject); 
            }
        }

        private void OnDisable()
        {
            if (_lifetimeCoroutine != null) StopCoroutine(_lifetimeCoroutine);
        }
    }
}