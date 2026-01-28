using UnityEngine;
using _60SecondsSurvivors.Enemy;

namespace _60SecondsSurvivors.Data
{
    [CreateAssetMenu(menuName = "60SecondsSurvivors/Data/Enemy Data", fileName = "EnemyData_")]
    public class EnemyData : ScriptableObject
    {
        public EnemyHealth prefab;
        public int maxHp = 3;
        public float moveSpeed = 2f;
        public int contactDamage = 1;
        public float damageTick = 1f;
    }
}

