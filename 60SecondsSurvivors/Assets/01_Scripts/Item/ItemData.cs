using UnityEngine;

namespace _60SecondsSurvivors.Item
{
    [CreateAssetMenu(menuName = "60SecondsSurvivors/Data/Item Data", fileName = "ItemData_")]
    public class ItemData : ScriptableObject
    {
        [Header("Prefab & UI")]
        public ItemBase prefab;

        [Tooltip("에디터에서 표시될 이름 (자동 동기화 가능)")]
        public string displayName;

        [Header("Type & Values")]
        public ItemType itemType;

        [Tooltip("비율형 값 (예: 0.15 = 15%)")]
        public float floatValue;

        [Tooltip("정수형 값 (예: 투사체 수, 관통 수)")]
        public int intValue;

        [Tooltip("지속시간 (무적 등)")]
        public float duration;

        private void Reset()
        {
            if (floatValue == 0f) floatValue = GetDefaultFloatForType(itemType);
            if (intValue == 0) intValue = GetDefaultIntForType(itemType);
            if (duration == 0f && itemType == ItemType.Invincible) duration = 3f;
            if (prefab != null && string.IsNullOrEmpty(displayName))
                displayName = prefab.name;
        }

        private static float GetDefaultFloatForType(ItemType t)
        {
            switch (t)
            {
                case ItemType.FireRate: return 0.15f;
                case ItemType.Damage: return 0.20f;
                case ItemType.MoveSpeed: return 0.10f;
                case ItemType.Heal: return 0.30f;
                default: return 0f;
            }
        }

        private static int GetDefaultIntForType(ItemType t)
        {
            switch (t)
            {
                case ItemType.ProjectileCount: return 1;
                case ItemType.Piercing: return 1;
                default: return 0;
            }
        }
    }
}