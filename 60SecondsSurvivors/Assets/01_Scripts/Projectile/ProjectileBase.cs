using UnityEngine;
using _60SecondsSurvivors.Enemy;

namespace _60SecondsSurvivors.Projectile
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBase : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifeTime = 2f;
        [SerializeField] private int _damage = 1;

        private Vector2 _direction = Vector2.right;
        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
                _direction = direction.normalized;
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = _direction * _speed;
        }

        private void Update()
        {
            _lifeTime -= Time.deltaTime;
            if (_lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}

