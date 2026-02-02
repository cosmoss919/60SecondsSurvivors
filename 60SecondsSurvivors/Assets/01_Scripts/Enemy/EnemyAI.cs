using UnityEngine;
using _60SecondsSurvivors.Player;
using _60SecondsSurvivors.Core;
using System.Collections;

namespace _60SecondsSurvivors.Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        private Animator animator;
        private int _maxHp;
        private int _currentHp;

        private float moveSpeed;
        private int damage;
        private float damageTick;

        private Transform target;
        private Rigidbody2D rigid;
        private SpriteRenderer spriteRenderer;
        private Coroutine _knockbackRoutine;
        private EnemyBase enemyBase;
        private Collider2D _collider;

        private EnemyState _state = EnemyState.Alive;

        public bool IsStunned { get; private set; }
        public bool IsAlive => _state == EnemyState.Alive;
        public bool IsDead => _state == EnemyState.Dead;

        private enum EnemyState
        {
            Alive,
            Dead
        }

        private void Awake()
        {
            enemyBase = GetComponent<EnemyBase>();
            if (enemyBase == null || enemyBase.Data == null)
            {
                GameLog.Error(this, "EnemyBase에 EnemyData가 없습니다");
                enabled = false;
                return;
            }

            animator = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();

            moveSpeed = enemyBase.Data.moveSpeed;
            damage = enemyBase.Data.contactDamage;
            damageTick = enemyBase.Data.damageTick;
            _maxHp = enemyBase.Data.maxHp;
            _currentHp = _maxHp;
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            if (!IsAlive) return;

            if (target == null) return;

            if (IsStunned)
                return;

            Vector2 direction = (target.position - transform.position).normalized;
            Vector2 nextVec = direction.normalized * moveSpeed * Time.deltaTime;
            rigid.MovePosition(rigid.position + nextVec);
            rigid.velocity = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (!IsAlive) return;

            if (target == null || spriteRenderer == null) return;
            spriteRenderer.flipX = target.position.x < transform.position.x;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsAlive) return;

            var playerController = collision.collider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                PlayerController.Instance?.StartContactDamage(gameObject, damage, damageTick);
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (!IsAlive) return;

            var playerController = other.collider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                PlayerController.Instance?.StopContactDamage(gameObject);
            }
        }

        public void OnSpawned()
        {
            _currentHp = _maxHp;
            _state = EnemyState.Alive;
            IsStunned = false;

            // 리셋: 콜라이더/애니메이터 상태
            if (_collider != null) _collider.enabled = true;

            if (animator != null)
            {
                animator.ResetTrigger("Hit");
                animator.SetBool("Dead", false);
            }
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            if (amount <= 0) return;

            if (animator != null)
                animator.SetTrigger("Hit");

            _currentHp -= amount;

            if (_currentHp <= 0)
            {
                _currentHp = 0;
                _state = EnemyState.Dead;
                StartCoroutine(Die());
            }
        }

        private IEnumerator Die()
        {
            _state = EnemyState.Dead;
            IsStunned = false;

            if (_collider != null)
                _collider.enabled = false;

            if (animator != null)
                animator.SetBool("Dead", true);

            ScoreManager.Instance?.AddScore(enemyBase.Data.score);

            yield return new WaitForSeconds(3f);

            PoolManager.Instance?.ReleaseToPool(gameObject);
        }

        public void ApplyKnockback(Vector2 sourcePos, float force = 2f, float duration = 0.12f)
        {
            if (!IsAlive) return;

            Vector2 dir = ((Vector2)transform.position - sourcePos).normalized;
            if (_knockbackRoutine != null)
                StopCoroutine(_knockbackRoutine);

            _knockbackRoutine = StartCoroutine(KnockbackRoutine(dir, force, duration));
        }

        private IEnumerator KnockbackRoutine(Vector2 dir, float force, float duration)
        {
            IsStunned = true;
            float t = 0f;

            while (t < duration)
            {
                float dt = Time.deltaTime;
                float k = Mathf.Lerp(force, 0f, t / duration);
                if (rigid != null)
                    rigid.MovePosition(rigid.position + dir * k * dt);
                else
                    transform.Translate((Vector3)(dir * k * dt), Space.World);

                t += dt;
                yield return null;
            }

            IsStunned = false;
            _knockbackRoutine = null;
        }

        private void OnDisable()
        {
            if (_knockbackRoutine != null)
            {
                StopCoroutine(_knockbackRoutine);
                _knockbackRoutine = null;
            }
            IsStunned = false;
            _state = EnemyState.Alive;
        }
    }
}

