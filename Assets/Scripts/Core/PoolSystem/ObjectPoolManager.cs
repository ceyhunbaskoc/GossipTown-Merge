using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.PoolSystem
{
    public class ObjectPoolManager : MonoBehaviour, IObjectPool
    {
        public static ObjectPoolManager Instance { get; private set; }

        [Header("Pool Configuration")]
        [SerializeField] private List<PoolMapping> _poolMappings;

        private Dictionary<PoolObjectType, IObjectPool<GameObject>> _pools;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitializePools()
        {
            _pools = new Dictionary<PoolObjectType, IObjectPool<GameObject>>();

            foreach (var mapping in _poolMappings)
            {
                if (mapping.prefab == null || mapping.type == PoolObjectType.None) continue;

                Transform container = new GameObject($"{mapping.type}_Container").transform;
                container.SetParent(transform);

                IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                    createFunc: () => CreatePooledItem(mapping.prefab, container), 
                    actionOnGet: OnTakeFromPool,                                   
                    actionOnRelease: OnReturnedToPool,                             
                    actionOnDestroy: OnDestroyPoolObject,                          
                    collectionCheck: true,                                         
                    defaultCapacity: mapping.initialSize,
                    maxSize: 500                                                   
                );

                _pools.Add(mapping.type, pool);

                var prewarmObjects = new List<GameObject>();
                for (int i = 0; i < mapping.initialSize; i++)
                {
                    prewarmObjects.Add(pool.Get());
                }
                foreach (var obj in prewarmObjects)
                {
                    pool.Release(obj);
                }
            }
        }

        private GameObject CreatePooledItem(GameObject prefab, Transform container)
        {
            GameObject obj = Instantiate(prefab, container);
            
            if (!obj.TryGetComponent<PooledObject>(out var pooledObj))
            {
                pooledObj = obj.AddComponent<PooledObject>();
            }
            return obj;
        }

        private void OnTakeFromPool(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnReturnedToPool(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void OnDestroyPoolObject(GameObject obj)
        {
            Destroy(obj);
        }

        public GameObject Spawn(PoolObjectType type, Vector3 position, Quaternion rotation, Transform parent = null, float lifetime = 0f)
        {
            if (!_pools.TryGetValue(type, out var pool))
            {
                Debug.LogError($"[ObjectPoolManager] Pool mapping for {type} is missing or not initialized!");
                return null;
            }

            GameObject obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            
            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            if (obj.TryGetComponent<PooledObject>(out var pooledObj))
            {
                pooledObj.Initialize(pool);
                pooledObj.Activate(lifetime);
            }

            return obj;
        }

        public T Spawn<T>(PoolObjectType type, Vector3 position, Quaternion rotation, float lifetime = 0f) where T : Component
        {
            GameObject obj = Spawn(type, position, rotation, null, lifetime);
            
            if (obj != null && obj.TryGetComponent<T>(out T component))
            {
                return component;
            }
            
            Debug.LogError($"[ObjectPoolManager] Component of type {typeof(T).Name} not found on pooled object {type}!");
            return null;
        }

        public T Spawn<T>(PoolObjectType type, Vector3 position, Quaternion rotation, Transform parent, float lifetime = 0f) where T : Component
        {
            GameObject obj = Spawn(type, position, rotation, parent, lifetime);
            
            if (obj != null && obj.TryGetComponent<T>(out T component))
            {
                return component;
            }
            
            Debug.LogError($"[ObjectPoolManager] Component of type {typeof(T).Name} not found on pooled object {type}!");
            return null;
        }
        
        public T SpawnUI<T>(PoolObjectType type, Transform parent=null, float lifetime = 0f) where T : Component
        {
            if (!_pools.TryGetValue(type, out var pool))
            {
                Debug.LogError($"[ObjectPoolManager] Pool mapping for {type} is missing or not initialized!");
                return null;
            }

            GameObject obj = pool.Get();

            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }

            if (obj.TryGetComponent<PooledObject>(out var pooledObj))
            {
                pooledObj.Initialize(pool);
                pooledObj.Activate(lifetime);
            }

            if (obj.TryGetComponent<T>(out T component))
            {
                RectTransform rectTransform = obj.transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localPosition = Vector3.zero;
                    rectTransform.localRotation = Quaternion.identity;
                    
                    Vector3 anchoredPos = rectTransform.anchoredPosition3D;
                    anchoredPos.z = 0f;
                    rectTransform.anchoredPosition3D = anchoredPos;
                }
                else
                {
                    Debug.LogWarning($"[ObjectPoolManager] SpawnUI called for {type}, but it lacks a RectTransform! Is this really a UI object?");
                }

                return component;
            }

            Debug.LogError($"[ObjectPoolManager] Component of type {typeof(T).Name} not found on pooled object {type}!");
            return null;
        }

        public void Despawn(PoolObjectType type, GameObject obj)
        {
            if (obj == null) return;

            if (_pools.TryGetValue(type, out var pool))
            {
                pool.Release(obj);
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] No pool found for {type}. Falling back to standard destruction.");
                Destroy(obj);
            }
        }

        public void Despawn<T>(PoolObjectType type, T component) where T : Component
        {
            if (component == null) return;
            Despawn(type, component.gameObject);
        }
    }
}