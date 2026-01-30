using System.Collections.Generic;
using UnityEngine;
using _60SecondsSurvivors.Enemy;
using _60SecondsSurvivors.Projectile;
using _60SecondsSurvivors.Item;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 풀 매니저
    /// </summary>
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
            DontDestroyOnLoad(gameObject);
        }

        public void Preload(Component prefab, int count = -1)
        {
            if (prefab == null) return;
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

                // 부모를 용도에 맞는 폴더 아래로 재설정
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

                // 생성 시 용도에 맞는 부모 아래로 둠
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

            EnsurePool(prefab);

            // 반환 시에도 올바른 부모(포폴더) 아래로 정리
            Transform parent = GetPoolParent(prefab);
            go.transform.SetParent(parent, false);

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

        // prefab 타입에 따라 프로젝트 뷰에서 보기 좋도록 부모(루트/종류별 폴더)를 리턴
        private Transform GetPoolParent(Component prefab)
        {
            string rootName;
            if (prefab is EnemyHealth)
                rootName = "Enemies";
            else if (prefab is ProjectileBase)
                rootName = "Weapons";
            else if(prefab is ItemBase)
                rootName = "Items";
            else
                rootName = "Pooled";

            // 루트 오브젝트 찾거나 생성
            GameObject root = GameObject.Find(rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
                DontDestroyOnLoad(root);
            }

            // prefab 별 서브 폴더(예: Enemies/Slime)
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

