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

    public class ItemBase : MonoBehaviour, IPoolable
    {
        private ItemData runtimeData;

        public ItemData Data => runtimeData;

        public void OnSpawned()
        {
        }

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
                GameLog.Error(this, "ItemData가 설정되어 있지 않습니다.");
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
                    weapon?.MultiplyFireRate(1f - fValue);
                    popupText = $"발사속도 +{Mathf.RoundToInt(fValue * 100f)}%";
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

            GameHUD.Instance?.ShowPickup(popupText);

            SoundManager.Instance?.PlayItemPickup();

        }
    }
}