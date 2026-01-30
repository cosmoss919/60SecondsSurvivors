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

    // 아이템 픽업 오브젝트 (풀링 지원)
    public class ItemBase : MonoBehaviour, IPoolable
    {
        public ItemType itemType;
        [Tooltip("비율형 값 (예: 0.15 = 15%)")]
        public float floatValue = 0.0f;
        [Tooltip("정수형 값 (예: 투사체 수, 관통 수)")]
        public int intValue = 0;
        [Tooltip("지속시간 (무적 등)")]
        public float duration = 0f;

        private void Reset()
        {
            ApplyTypeDefaults();
        }

        private void OnValidate()
        {
            ApplyTypeDefaults();
        }

        private void ApplyTypeDefaults()
        {
            // 에디터에서 값을 직접 바꿀 수 있으니, 0으로 남아있는 경우에만 기본값을 채움
            switch (itemType)
            {
                case ItemType.FireRate:
                    if (floatValue <= 0f) floatValue = 0.15f; // 발사간격 15% 감소
                    break;
                case ItemType.Damage:
                    if (floatValue <= 0f) floatValue = 0.20f; // 데미지 +20%
                    break;
                case ItemType.MoveSpeed:
                    if (floatValue <= 0f) floatValue = 0.10f; // 이동속도 +10%
                    break;
                case ItemType.ProjectileCount:
                    if (intValue <= 0) intValue = 1; // 투사체 +1
                    break;
                case ItemType.Piercing:
                    if (intValue <= 0) intValue = 1; // 관통 +1
                    break;
                case ItemType.Heal:
                    if (floatValue <= 0f) floatValue = 0.30f; // 체력 30% 회복
                    break;
                case ItemType.Invincible:
                    if (duration <= 0f) duration = 3f; // 무적 3초
                    break;
            }
        }

        public void OnSpawned()
        {
            // 필요하면 리셋 로직 추가
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyToPlayer(player);
                // Item은 풀에 넣지 않고 비활성화 처리(현재 PoolManager는 Item은 Instantiate/Disable 처리)
                if (PoolManager.Instance != null)
                    PoolManager.Instance.ReleaseToPool(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }

        private void ApplyToPlayer(PlayerController player)
        {
            var weapon = player.GetComponentInChildren<PlayerWeapon>();
            string popupText = "";

            switch (itemType)
            {
                case ItemType.FireRate:
                    weapon?.MultiplyFireRate(1f - floatValue); // 공속 증가: 인터벌 감소
                    popupText = $"공속 +{Mathf.RoundToInt(floatValue * 100f)}%";
                    break;
                case ItemType.Damage:
                    weapon?.MultiplyDamage(1f + floatValue);
                    popupText = $"데미지 +{Mathf.RoundToInt(floatValue * 100f)}%";
                    break;
                case ItemType.MoveSpeed:
                    player.AddMoveSpeedPercent(floatValue);
                    popupText = $"이동속도 +{Mathf.RoundToInt(floatValue * 100f)}%";
                    break;
                case ItemType.ProjectileCount:
                    weapon?.AddProjectileCount(intValue);
                    popupText = $"+{intValue} 발";
                    break;
                case ItemType.Piercing:
                    weapon?.AddPierce(intValue);
                    popupText = $"관통 +{intValue}";
                    break;
                case ItemType.Heal:
                    player.HealPercent(floatValue);
                    popupText = $"HP 회복 {Mathf.RoundToInt(floatValue * 100f)}%";
                    break;
                case ItemType.Invincible:
                    player.StartInvincible(duration);
                    popupText = $"무적 {duration:0.#}초";
                    break;
            }

            GameHUD.Instance?.ShowPickup(popupText);
        }
    }
}