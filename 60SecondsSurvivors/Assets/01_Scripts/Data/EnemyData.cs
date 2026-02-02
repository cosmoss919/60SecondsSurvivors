using UnityEngine;
using _60SecondsSurvivors.Enemy;

namespace _60SecondsSurvivors.Data
{
    [CreateAssetMenu(menuName = "60SecondsSurvivors/Data/Enemy Data", fileName = "EnemyData_")]
    public class EnemyData : ScriptableObject
    {
        public EnemyAI prefab;
        public int maxHp = 3;
        public float moveSpeed = 2f;
        public int contactDamage = 1;
        public float damageTick = 1f;

        [Header("Score")]
        [Tooltip("이 적을 처치했을 때 부여되는 점수")]
        public int score = 10;
    }
}

