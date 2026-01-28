using UnityEngine;
using _60SecondsSurvivors.Player;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        private float _moveSpeed;
        private int _damage;
        private float _damageTick;

        private Transform _target;
        private PlayerHealth _playerHealth;
        private float _damageTimer;

        private void Awake()
        {
            var enemyBase = GetComponent<EnemyBase>();
            if (enemyBase == null || enemyBase.Data == null)
            {
                GameLog.Error(this, "EnemyBase에 EnemyData가 없습니다");
                enabled = false;
                return;
            }

            _moveSpeed = enemyBase.Data.moveSpeed;
            _damage = enemyBase.Data.contactDamage;
            _damageTick = enemyBase.Data.damageTick;
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
        }

        private void Update()
        {
            if (_target == null) return;

            var direction = (_target.position - transform.position).normalized;
            transform.position += direction * (_moveSpeed * Time.deltaTime);

            if (_playerHealth == null || _damageTick <= 0f) return;

            _damageTimer += Time.deltaTime;
            if (_damageTimer >= _damageTick)
            {
                _damageTimer = 0f;
                _playerHealth.TakeDamage(_damage);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other) 
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                _playerHealth = playerHealth;
                _damageTimer = 1f;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_playerHealth == null) return;

            if (other.GetComponent<PlayerHealth>() == _playerHealth)
            {
                _playerHealth = null;
                _damageTimer = 0f;
            }
        }
    }
}

