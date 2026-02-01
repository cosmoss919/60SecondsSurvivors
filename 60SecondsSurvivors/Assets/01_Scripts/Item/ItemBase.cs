using UnityEngine;
using _60SecondsSurvivors.Player;
using _60SecondsSurvivors.Core;
using _60SecondsSurvivors.UI;

namespace _60SecondsSurvivors.Item
{
    public enum ItemType
    {
        FireRate,
        Damage,
        MoveSpeed,
        ProjectileCount,
        Piercing,
        Heal,
        Invincible
    }

    /// <summary>
    /// 아이템 픽업 오브젝트 (풀링 호환)
    /// Runtime에 ItemData를 주입받아 동작
    /// </summary>
    public class ItemBase : MonoBehaviour, IPoolable
    {
        // 런타임에 주입되는 데이터 (직렬화하지 않음)
        private ItemData runtimeData;

        public ItemData Data => runtimeData;

        public void OnSpawned()
        {
            // 필요하면 시각/상태 초기화
        }

        // ItemDropper가 드롭 시 호출해서 데이터 주입
        public void SetData(ItemData data)
        {
            runtimeData = data;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyToPlayer(player);

                // Item은 풀에 넣지 않고 비활성화 방식(현재 PoolManager는 Item을 Instantiate 후 Disable 처리)
                if (PoolManager.Instance != null)
                    PoolManager.Instance.ReleaseToPool(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }

        private void ApplyToPlayer(PlayerController player)
        {
            ItemType type;
            float fValue;
            int iValue;
            float duration;

            if (runtimeData == null)
            {
                GameLog.Error(this, "ItemData가 없습니다");
            }

            type = runtimeData.itemType;
            fValue = runtimeData.floatValue;
            iValue = runtimeData.intValue;
            duration = runtimeData.duration;

            var weapon = player.GetComponentInChildren<PlayerWeapon>();
            string popupText = "";

            switch (type)
            {
                case ItemType.FireRate:
                    weapon?.MultiplyFireRate(1f - fValue); // 인터벌 감소(공속 증가)
                    popupText = $"공속 +{Mathf.RoundToInt(fValue * 100f)}%";
                    break;
                case ItemType.Damage:
                    weapon?.MultiplyDamage(1f + fValue);
                    popupText = $"데미지 +{Mathf.RoundToInt(fValue * 100f)}%";
                    break;
                case ItemType.MoveSpeed:
                    player.AddMoveSpeedPercent(fValue);
                    popupText = $"이동속도 +{Mathf.RoundToInt(fValue * 100f)}%";
                    break;
                case ItemType.ProjectileCount:
                    weapon?.AddProjectileCount(iValue);
                    popupText = $"+{iValue} 발";
                    break;
                case ItemType.Piercing:
                    weapon?.AddPierce(iValue);
                    popupText = $"관통 +{iValue}";
                    break;
                case ItemType.Heal:
                    player.HealPercent(fValue);
                    popupText = $"HP 회복 {Mathf.RoundToInt(fValue * 100f)}%";
                    break;
                case ItemType.Invincible:
                    player.StartInvincible(duration);
                    popupText = $"무적 {duration:0.#}초";
                    break;
            }

            // HUD에 팝업 표시 (ScriptableObject의 displayName이 있으면 우선 사용)
            string display = runtimeData != null && !string.IsNullOrEmpty(runtimeData.displayName) ? runtimeData.displayName : popupText;
            GameHUD.Instance?.ShowPickup(display);
        }
    }
}