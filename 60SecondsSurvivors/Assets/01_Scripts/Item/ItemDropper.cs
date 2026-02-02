using UnityEngine;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Item
{
    public class ItemDropper : MonoBehaviour
    {
        public static ItemDropper Instance { get; private set; }

        [Tooltip("드랍 확률 (0~1)")]
        [Range(0f, 1f)]
        public float dropChance = 0.35f;

        [Tooltip("아이템 데이터 목록 (ItemData SO, prefab 필드에 ItemBase 프리팹 연결)")]
        public ItemData[] itemDatas;

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

        public void TryDrop(Vector3 position)
        {
            if (itemDatas == null || itemDatas.Length == 0) return;

            if (Random.value > dropChance) return;

            // 랜덤 아이템 선택
            var data = itemDatas[Random.Range(0, itemDatas.Length)];
            if (data == null || data.prefab == null) return;

            GameObject go = PoolManager.Instance != null ? PoolManager.Instance.GetFromPool(data.prefab) : Instantiate(data.prefab.gameObject);
            if (go == null) return;

            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            // 런타임으로 ItemData 주입
            var itemBase = go.GetComponent<ItemBase>();
            if (itemBase != null)
            {
                itemBase.SetData(data);
            }
        }
    }
}