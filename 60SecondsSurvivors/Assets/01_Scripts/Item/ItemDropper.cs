using UnityEngine;
using _60SecondsSurvivors.Core;
using _60SecondsSurvivors.Item;

namespace _60SecondsSurvivors.Item
{
    // 적 처치 시 아이템 드랍을 담당
    public class ItemDropper : MonoBehaviour
    {
        public static ItemDropper Instance { get; private set; }

        [Tooltip("드랍 확률 (0~1)")]
        [Range(0f, 1f)]
        public float dropChance = 0.35f;

        [Tooltip("아이템 프리팹 목록 (ItemBase가 붙어 있어야 함)")]
        public ItemBase[] itemPrefabs;

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

        private void Start()
        {
            //if (PoolManager.Instance != null && itemPrefabs != null)
            //{
            //    foreach (var p in itemPrefabs)
            //    {
            //        if (p != null)
            //            PoolManager.Instance.Preload(p);
            //    }
            //}
        }

        public void TryDrop(Vector3 position)
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0) return;

            if (Random.value > dropChance) return;

            // 랜덤 아이템 선택
            var prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            if (prefab == null) return;

            GameObject go = PoolManager.Instance != null ? PoolManager.Instance.GetFromPool(prefab) : Instantiate(prefab.gameObject);
            if (go == null) return;

            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;
        }
    }
}