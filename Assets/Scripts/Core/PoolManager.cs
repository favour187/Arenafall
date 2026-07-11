using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ArenaFall.Core
{
    /// <summary>
    /// Generic object pooling system for frequently spawned objects.
    /// Reduces GC allocations and improves performance.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        [System.Serializable]
        public class PoolConfig
        {
            public string poolId;
            public GameObject prefab;
            public int defaultCapacity = 10;
            public int maxSize = 50;
            public bool collectionChecks = false;
        }

        [SerializeField] private List<PoolConfig> _pools = new();
        private readonly Dictionary<string, IObjectPool<GameObject>> _poolDict = new();
        private readonly Dictionary<string, PoolConfig> _configDict = new();
        private readonly Dictionary<GameObject, string> _activeObjectMap = new();

        public static PoolManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var config in _pools)
            {
                if (!_configDict.ContainsKey(config.poolId))
                {
                    _configDict[config.poolId] = config;
                    _poolDict[config.poolId] = new ObjectPool<GameObject>(
                        () => CreatePooledObject(config),
                        OnGetFromPool,
                        OnReleaseToPool,
                        OnDestroyPooledObject,
                        config.collectionChecks,
                        config.defaultCapacity,
                        config.maxSize
                    );
                }
            }
            Debug.Log($"[PoolManager] Initialized {_poolDict.Count} pools");
        }

        private GameObject CreatePooledObject(PoolConfig config)
        {
            GameObject obj = Instantiate(config.prefab);
            obj.SetActive(false);
            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.PoolId = config.poolId;
            }
            return obj;
        }

        private void OnGetFromPool(GameObject obj)
        {
            obj.SetActive(true);
            _activeObjectMap[obj] = FindPoolIdForObject(obj);
        }

        private void OnReleaseToPool(GameObject obj)
        {
            obj.SetActive(false);
            _activeObjectMap.Remove(obj);
        }

        private void OnDestroyPooledObject(GameObject obj)
        {
            Destroy(obj);
            _activeObjectMap.Remove(obj);
        }

        private string FindPoolIdForObject(GameObject obj)
        {
            if (obj.TryGetComponent<IPoolable>(out var poolable) && !string.IsNullOrEmpty(poolable.PoolId))
            {
                return poolable.PoolId;
            }
            foreach (var kvp in _configDict)
            {
                if (kvp.Value.prefab.name == obj.name.Replace("(Clone)", "").Trim())
                {
                    return kvp.Key;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        public GameObject Get(string poolId, Vector3 position, Quaternion rotation)
        {
            if (!_poolDict.ContainsKey(poolId))
            {
                Debug.LogError($"[PoolManager] Pool '{poolId}' not found!");
                return null;
            }

            GameObject obj = _poolDict[poolId].Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        /// <summary>
        /// Get an object from the pool with a parent transform.
        /// </summary>
        public GameObject Get(string poolId, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject obj = Get(poolId, position, rotation);
            if (obj != null)
            {
                obj.transform.SetParent(parent);
            }
            return obj;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void Release(GameObject obj)
        {
            if (obj == null) return;

            if (obj.TryGetComponent<IPoolable>(out var poolable) && !string.IsNullOrEmpty(poolable.PoolId))
            {
                if (_poolDict.ContainsKey(poolable.PoolId))
                {
                    _poolDict[poolable.PoolId].Release(obj);
                    return;
                }
            }

            if (_activeObjectMap.TryGetValue(obj, out var poolId) && _poolDict.ContainsKey(poolId))
            {
                _poolDict[poolId].Release(obj);
                return;
            }

            // Fallback: find by config
            foreach (var kvp in _configDict)
            {
                if (obj.name.StartsWith(kvp.Value.prefab.name))
                {
                    if (_poolDict.ContainsKey(kvp.Key))
                    {
                        _poolDict[kvp.Key].Release(obj);
                        return;
                    }
                }
            }

            // If not tracked, destroy
            Destroy(obj);
        }

        /// <summary>
        /// Pre-warm a pool with a number of objects.
        /// </summary>
        public void PreWarm(string poolId, int count)
        {
            if (!_poolDict.ContainsKey(poolId)) return;

            var pool = _poolDict[poolId];
            var tempList = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                tempList.Add(pool.Get());
            }
            foreach (var obj in tempList)
            {
                pool.Release(obj);
            }
        }

        /// <summary>
        /// Clear all pools.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _poolDict.Values)
            {
                pool.Clear();
            }
            _activeObjectMap.Clear();
            Debug.Log("[PoolManager] All pools cleared");
        }
    }

    /// <summary>
    /// Interface for poolable objects.
    /// </summary>
    public interface IPoolable
    {
        string PoolId { get; set; }
        void OnPoolGet();
        void OnPoolRelease();
    }
}
