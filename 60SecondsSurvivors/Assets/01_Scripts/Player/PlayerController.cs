using _60SecondsSurvivors.Core;
using UnityEngine;

namespace _60SecondsSurvivors.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public Vector2 Position => transform.position;
        public Vector2 InputVec => inputVec;
        public int Health => currentHp;

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private int maxHp = 100;
        private int currentHp;
        private Animator animator;
        private Rigidbody2D rigid;
        private SpriteRenderer spriteRenderer;
        private Vector2 inputVec;

        private void Awake()
        {
            currentHp = maxHp;
            animator = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

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

            currentHp -= amount;

            if (currentHp <= 0)
            {
                currentHp = 0;
                Die();
            }
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