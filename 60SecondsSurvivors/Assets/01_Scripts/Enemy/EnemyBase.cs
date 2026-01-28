using UnityEngine;
using _60SecondsSurvivors.Data;

namespace _60SecondsSurvivors.Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;

        public EnemyData Data => _data;
    }
}

