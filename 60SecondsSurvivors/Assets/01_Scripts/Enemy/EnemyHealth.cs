using UnityEngine;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Enemy
{
    /// <summary>
    /// 적 체력을 관리합니다.
    /// 풀링 지원: OnSpawned, Die에서 PoolManager로 반환
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        private Animator animator;
        private int _maxHp;
        private int _currentHp;

        private void Awake()
        {
            animator = GetComponent<Animator>();
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

        public void OnSpawned()
        {
            _currentHp = _maxHp;
            if (animator != null)
            {
                animator.ResetTrigger("Hit");
                animator.SetBool("Dead", false);
            }
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            animator.SetTrigger("Hit");
            _currentHp -= amount;

            if (_currentHp <= 0)
            {
                _currentHp = 0;
                Die();
            }
        }

        private void Die()
        {
            animator.SetBool("Dead", true);
            PoolManager.Instance.ReleaseToPool(gameObject);
        }
    }
}

