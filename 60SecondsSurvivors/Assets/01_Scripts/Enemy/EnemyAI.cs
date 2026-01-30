using UnityEngine;
using _60SecondsSurvivors.Player;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        private float moveSpeed;
        private int damage;
        private float damageTick;

        private Transform target;
        private PlayerController playerController;
        private Rigidbody2D rigid;
        private SpriteRenderer spriteRenderer;
        private float damageTimer;

        private void Awake()
        {
            var enemyBase = GetComponent<EnemyBase>();
            if (enemyBase == null || enemyBase.Data == null)
            {
                GameLog.Error(this, "EnemyBase에 EnemyData가 없습니다");
                enabled = false;
                return;
            }

            rigid = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            moveSpeed = enemyBase.Data.moveSpeed;
            damage = enemyBase.Data.contactDamage;
            damageTick = enemyBase.Data.damageTick;
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

            if (target == null) return;

            Vector2 direction = (target.position - transform.position).normalized;
            Vector2 nextVec = direction.normalized * moveSpeed * Time.deltaTime;
            rigid.MovePosition(rigid.position + nextVec);
            rigid.velocity = Vector2.zero;

            TakeDamage();
        }

        private void LateUpdate()
        {
            spriteRenderer.flipX = target.position.x < transform.position.x;
        }

        private void TakeDamage()
        {
            if (playerController == null || damageTick <= 0f) return;

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageTick)
            {
                damageTimer = 0f;
                playerController.TakeDamage(damage);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var playerController = collision.collider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                this.playerController = playerController;
                damageTimer = 1f;
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (playerController == null) return;

            if (other.collider.GetComponent<PlayerController>() == playerController)
            {
                playerController = null;
                damageTimer = 0f;
            }
        }
    }
}

