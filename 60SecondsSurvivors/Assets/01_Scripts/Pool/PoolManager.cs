using System.Collections.Generic;
using UnityEngine;
using _60SecondsSurvivors.Enemy;
using _60SecondsSurvivors.Projectile;
using _60SecondsSurvivors.Item;

namespace _60SecondsSurvivors.Core
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [SerializeField] private int defaultMaxPoolCount = 30;

        private readonly Dictionary<Component, Queue<GameObject>> pools = new();
        private readonly Dictionary<Component, int> createdCounts = new();
        private readonly Dictionary<GameObject, Component> instanceToPrefab = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Preload(Component prefab, int count = -1)
        {
            if (prefab == null) return;

            if (prefab is ItemBase)
                return;

            int toCreate = count <= 0 ? defaultMaxPoolCount : count;
            EnsurePool(prefab);

            Transform parent = GetPoolParent(prefab);

            for (int i = 0; i < toCreate; i++)
            {
                GameObject go = Instantiate(prefab.gameObject);
                go.SetActive(false);
                go.transform.SetParent(parent, false);

                pools[prefab].Enqueue(go);

                instanceToPrefab[go] = prefab;
                createdCounts[prefab]++;
            }
        }

        public GameObject GetFromPool(Component prefab)
        {
            if (prefab == null) return null;
            EnsurePool(prefab);

            Queue<GameObject> pool = pools[prefab];
            Transform parent = GetPoolParent(prefab);

            if (pool.Count > 0)
            {
                GameObject go = pool.Dequeue();
                go.SetActive(true);
                instanceToPrefab[go] = prefab;

                go.transform.SetParent(parent, false);

                var poolable = go.GetComponent<IPoolable>();
                poolable?.OnSpawned();

                return go;
            }

            if (!createdCounts.TryGetValue(prefab, out int created))
                created = 0;

            if (created < defaultMaxPoolCount)
            {
                GameObject go = Instantiate(prefab.gameObject);
                instanceToPrefab[go] = prefab;
                createdCounts[prefab] = created + 1;
                go.SetActive(true);

                go.transform.SetParent(parent, false);

                var poolable = go.GetComponent<IPoolable>();
                poolable?.OnSpawned();

                return go;
            }

            return null;
        }

        public void ReleaseToPool(GameObject go)
        {
            if (go == null) return;

            if (!instanceToPrefab.TryGetValue(go, out Component prefab) || prefab == null)
            {
                Destroy(go);
                return;
            }

            // Item은 풀에 다시 넣지 않고 단순 비활성화 및 정리만 수행
            if (prefab is ItemBase)
            {
                Transform parent = GetPoolParent(prefab);
                go.transform.SetParent(parent, false);
                go.SetActive(false);
                // instanceToPrefab는 유지해도 무방
                return;
            }

            EnsurePool(prefab);

            // 반환 시에도 올바른 부모(폴더) 아래로 정리
            Transform parentPool = GetPoolParent(prefab);
            go.transform.SetParent(parentPool, false);

            go.SetActive(false);
            pools[prefab].Enqueue(go);
        }

        private void EnsurePool(Component prefab)
        {
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }

            if (!createdCounts.ContainsKey(prefab))
            {
                createdCounts[prefab] = 0;
            }
        }

        private Transform GetPoolParent(Component prefab)
        {
            string rootName;
            if (prefab is EnemyAI)
                rootName = "Enemies";
            else if (prefab is ProjectileBase)
                rootName = "Weapons";
            else if (prefab is ItemBase)
                rootName = "Items";
            else
                rootName = "Pooled";

            GameObject root = GameObject.Find(rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
            }

            string childName = prefab.gameObject.name;
            Transform child = root.transform.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(root.transform, false);
                child = go.transform;
            }

            return child;
        }
    }
}

