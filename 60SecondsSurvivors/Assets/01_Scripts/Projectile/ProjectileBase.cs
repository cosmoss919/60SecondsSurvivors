using UnityEngine;
using _60SecondsSurvivors.Enemy;
using _60SecondsSurvivors.Core;

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

        // 넉백 세팅 (프리팹 단위로 조절 가능)
        [SerializeField] private float knockbackForce = 2f;
        [SerializeField] private float knockbackDuration = 0.12f;

        private Vector2 direction = Vector2.right;
        private Rigidbody2D rigid;
        private float initialLifeTime;
        private int initialDamage;
        private int initialPierce;

        // Job 기반 이동 사용 여부 (기본: true)
        [SerializeField] private bool useJobMovement = true;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
            initialLifeTime = lifeTime;
            initialDamage = damage;
            initialPierce = pierce;
        }

        public void OnSpawned()
        {
            lifeTime = initialLifeTime;
            damage = initialDamage;
            pierce = initialPierce;
            if (rigid != null && !useJobMovement)
                rigid.velocity = Vector2.zero;

            // Register to mover if available and using job movement
            if (useJobMovement && ProjectileMover.Instance != null)
                ProjectileMover.Instance.Register(this);
        }

        private void OnDisable()
        {
            // Unregister from mover
            if (useJobMovement && ProjectileMover.Instance != null)
                ProjectileMover.Instance.Unregister(this);
        }

        private void OnDestroy()
        {
            if (useJobMovement && ProjectileMover.Instance != null)
                ProjectileMover.Instance.Unregister(this);
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
                this.direction = direction.normalized;
        }

        // 외부(ProjectileMover)에서 읽을 수 있도록 velocity 반환
        public Vector3 GetVelocity()
        {
            return (Vector3)direction * speed;
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
            // 기존 물리 이동은 사용하지 않음 when job mover present
            if (useJobMovement && ProjectileMover.Instance != null)
            {
                if (rigid != null)
                    rigid.velocity = Vector2.zero;
                return;
            }

            if (rigid != null)
                rigid.velocity = direction * speed;
        }

        private void Update()
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                if (useJobMovement && ProjectileMover.Instance != null)
                    ProjectileMover.Instance.Unregister(this);

                if (PoolManager.Instance != null)
                    PoolManager.Instance.ReleaseToPool(gameObject);
                else
                    Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemyAI = other.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage);

                if (enemyAI.isActiveAndEnabled)
                {
                    enemyAI.ApplyKnockback(transform.position, knockbackForce, knockbackDuration);
                }

                if (pierce > 0)
                {
                    pierce--;
                    return;
                }

                if (useJobMovement && ProjectileMover.Instance != null)
                    ProjectileMover.Instance.Unregister(this);

                if (PoolManager.Instance != null)
                    PoolManager.Instance.ReleaseToPool(gameObject);
                else
                    Destroy(gameObject);
            }
        }
    }
}