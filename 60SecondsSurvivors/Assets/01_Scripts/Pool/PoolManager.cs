using System.Collections.Generic;
using UnityEngine;
using _60SecondsSurvivors.Enemy;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 풀링 매니저
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [SerializeField] private int _defaultMaxPoolCount = 30;

        private readonly Dictionary<EnemyHealth, Queue<GameObject>> _pools = new();
        private readonly Dictionary<EnemyHealth, int> _createdCounts = new();
        private readonly Dictionary<GameObject, EnemyHealth> _instanceToPrefab = new();

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

        public void Preload(EnemyHealth prefab, int count = -1)
        {
            if (prefab == null) return;
            int toCreate = count <= 0 ? _defaultMaxPoolCount : count;
            EnsurePool(prefab);

            for (int i = 0; i < toCreate; i++)
            {
                GameObject go = Instantiate(prefab.gameObject);
                go.SetActive(false);
                _pools[prefab].Enqueue(go);

                _instanceToPrefab[go] = prefab;
                _createdCounts[prefab]++;
            }
        }

        public GameObject GetFromPool(EnemyHealth prefab)
        {
            if (prefab == null) return null;

            EnsurePool(prefab);

            Queue<GameObject> pool = _pools[prefab];

            if (pool.Count > 0)
            {
                GameObject go = pool.Dequeue();
                go.SetActive(true);
                EnemyHealth eh = go.GetComponent<EnemyHealth>();
                eh?.OnSpawned();
                return go;
            }

            if (_createdCounts.TryGetValue(prefab, out int created) == false)
                created = 0;

            if (created < _defaultMaxPoolCount)
            {
                GameObject go = Instantiate(prefab.gameObject);
                _instanceToPrefab[go] = prefab;
                _createdCounts[prefab] = created + 1;
                go.SetActive(true);
                EnemyHealth eh = go.GetComponent<EnemyHealth>();
                eh?.OnSpawned();
                return go;
            }

            return null;
        }

        public void ReleaseToPool(GameObject go)
        {
            if (go == null) return;

            if (!_instanceToPrefab.TryGetValue(go, out EnemyHealth prefab) || prefab == null)
            {
                Destroy(go);
                return;
            }

            EnsurePool(prefab);

            go.SetActive(false);
            _pools[prefab].Enqueue(go);
        }

        private void EnsurePool(EnemyHealth prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new Queue<GameObject>();
            }

            if (!_createdCounts.ContainsKey(prefab))
            {
                _createdCounts[prefab] = 0;
            }
        }
    }
}

