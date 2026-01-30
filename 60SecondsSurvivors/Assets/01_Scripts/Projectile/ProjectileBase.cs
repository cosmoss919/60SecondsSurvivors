using _60SecondsSurvivors.Core;
using _60SecondsSurvivors.Enemy;
using UnityEngine;

namespace _60SecondsSurvivors.Projectile
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBase : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private int damage = 1;
        [SerializeField] private int pierce = 0; // 관통 수(0 = 관통 없음)

        private Vector2 direction = Vector2.right;
        private Rigidbody2D rigid;
        private float initialLifeTime;
        private int initialDamage;
        private int initialPierce;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
            initialLifeTime = lifeTime;
            initialDamage = damage;
            initialPierce = pierce;
        }

        public void OnSpawned()
        {
            // 풀에서 꺼낼 때 기본값으로 복원
            lifeTime = initialLifeTime;
            damage = initialDamage;
            pierce = initialPierce;
            if (rigid != null)
                rigid.velocity = Vector2.zero;
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
                this.direction = direction.normalized;
        }

        public void MultiplyDamage(float factor)
        {
            damage = Mathf.Max(1, Mathf.RoundToInt(initialDamage * factor));
        }

        public void SetPierce(int count)
        {
            pierce = count;
        }

        private void FixedUpdate()
        {
            if (rigid != null)
                rigid.velocity = direction * speed;
        }

        private void Update()
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                if (PoolManager.Instance != null)
                    PoolManager.Instance.ReleaseToPool(gameObject);
                else
                    Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                if (pierce > 0)
                {
                    pierce--;
                    // 관통 남음: 그대로 유지 (충돌한 적은 데미지 입고 투사체는 계속)
                    return;
                }

                if (PoolManager.Instance != null)
                    PoolManager.Instance.ReleaseToPool(gameObject);
                else
                    Destroy(gameObject);
            }
        }
    }
}