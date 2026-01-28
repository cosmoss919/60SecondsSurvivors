using UnityEngine;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int _maxHp = 3;
        private int _currentHp;
        
        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;

        private void Awake()
        {
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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied();
            }

            // 일단은 바로 제거. 나중에 죽는 연출 추가 가능
            Destroy(gameObject);
        }
    }
}

