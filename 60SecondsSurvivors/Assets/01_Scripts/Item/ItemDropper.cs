using UnityEngine;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Item
{
    public class ItemDropper : MonoBehaviour
    {
        public static ItemDropper Instance { get; private set; }

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

        public void TryDrop(Vector3 position, float enemyDropChance = -1f)
        {
            if (itemDatas == null || itemDatas.Length == 0) return;
            if (Random.value > enemyDropChance) return;

            // 랜덤 아이템 선택
            var data = itemDatas[Random.Range(0, itemDatas.Length)];
            if (data == null || data.prefab == null) return;

            GameObject go = PoolManager.Instance != null ? PoolManager.Instance.GetFromPool(data.prefab) : Instantiate(data.prefab.gameObject);
            if (go == null) return;

            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            var itemBase = go.GetComponent<ItemBase>();
            if (itemBase != null)
            {
                itemBase.SetData(data);
            }

            SoundManager.Instance?.PlayItemSpawn();
        }
    }
}