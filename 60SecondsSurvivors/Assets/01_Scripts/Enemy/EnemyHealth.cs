using UnityEngine;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Enemy
{
    /// <summary>
    /// 적 체력을 관리합니다.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        private int _maxHp;
        private int _currentHp;

        private void Awake()
        {
            var enemyBase = GetComponent<EnemyBase>();
            if (enemyBase == null || enemyBase.Data == null)
            {
                GameLog.Error(this, "EnemyBase에 EnemyData가 없습니다");
                enabled = false;
                return;
            }

            _maxHp = enemyBase.Data.maxHp;
            _currentHp = _maxHp;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            _currentHp -= amount;

            if (_currentHp <= 0)
            {
                _currentHp = 0;
                Die();
            }
        }

        private void Die()
        {
            // TODO: 점수, 아이템 드랍 등 추후 연동
            Destroy(gameObject);
        }
    }
}

