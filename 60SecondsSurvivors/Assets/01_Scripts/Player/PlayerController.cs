using _60SecondsSurvivors.Core;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace _60SecondsSurvivors.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }
        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        public Vector2 Position => transform.position;
        public Vector2 InputVec => inputVec;

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float maxHp = 100;
        private float currentHp;
        private Animator animator;
        private Rigidbody2D rigid;
        private SpriteRenderer spriteRenderer;
        private Material material;
        private Vector2 inputVec;

        // 무적 상태
        private bool isInvincible;
        private Coroutine invincibleRoutine;

        private void Awake()
        {
            currentHp = maxHp;
            animator = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            material = spriteRenderer.material;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void Update()
        {
            inputVec.x = Input.GetAxisRaw("Horizontal");
            inputVec.y = Input.GetAxisRaw("Vertical");
            inputVec = inputVec.normalized;
        }
        private void FixedUpdate()
        {
            rigid.velocity = inputVec * moveSpeed;
        }

        private void LateUpdate()
        {
            animator.SetFloat("Speed", inputVec.magnitude);

            if (inputVec.x != 0)
            {
                spriteRenderer.flipX = inputVec.x < 0;
            }
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;
            if (isInvincible) return;

            currentHp -= amount;

            if (currentHp <= 0)
            {
                currentHp = 0;
                Die();
            }
        }

        public void HealPercent(float percent)
        {
            if (percent <= 0) return;
            int heal = Mathf.CeilToInt(maxHp * percent);
            currentHp += heal;
            if (currentHp > maxHp) currentHp = maxHp;
        }

        public void AddMoveSpeedPercent(float percent)
        {
            if (percent == 0) return;
            moveSpeed *= (1f + percent);
        }

        public void StartInvincible(float duration)
        {
            if (invincibleRoutine != null)
            {
                StopCoroutine(invincibleRoutine);
            }
            invincibleRoutine = StartCoroutine(InvincibleCoroutine(duration));
        }

        private IEnumerator InvincibleCoroutine(float duration)
        {
            isInvincible = true;
            material.SetFloat("_Invincible", 1f);
            yield return new WaitForSecondsRealtime(duration);
            material.SetFloat("_Invincible", 0);
            isInvincible = false;
            invincibleRoutine = null;
        }

        private void Die()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied();
            }

            animator.SetTrigger("Dead");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}