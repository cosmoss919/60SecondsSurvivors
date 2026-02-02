using _60SecondsSurvivors.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _60SecondsSurvivors.UI;

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
        [SerializeField] private VirtualJoystick _joystick;

        private float hitFlashDuration = 0.2f;
        private float currentHp;
        private Animator animator;
        private Rigidbody2D rigid;
        private SpriteRenderer spriteRenderer;
        private Material material;
        private Vector2 inputVec;

        // 무적 상태
        private bool isInvincible;
        private Coroutine invincibleRoutine;

        // 피격 플래시 코루틴 참조
        private Coroutine hitFlashRoutine;

        // 접촉 데미지 코루틴 관리 (적별로 코루틴 유지)
        private readonly Dictionary<GameObject, Coroutine> _contactCoroutines = new();

        private void Awake()
        {
            currentHp = maxHp;
            animator = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            if (spriteRenderer != null)
            {
                // SpriteRenderer의 sharedMaterial 대신 인스턴스화된 material 사용
                material = spriteRenderer.material;
            }

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // 게임 오버이면 모든 입력/이동/접촉 데미지 중지 (애니메이션은 계속 재생)
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                inputVec = Vector2.zero;
                if (rigid != null)
                    rigid.velocity = Vector2.zero;

                // 접촉 데미지 코루틴 정리
                if (_contactCoroutines.Count > 0)
                {
                    foreach (var kv in _contactCoroutines)
                    {
                        if (kv.Value != null)
                            StopCoroutine(kv.Value);
                    }
                    _contactCoroutines.Clear();
                }

                return;
            }

            if (_joystick != null && _joystick.Direction != Vector2.zero)
            {
                inputVec = _joystick.Direction;
            }
            else
            {
                inputVec.x = Input.GetAxisRaw("Horizontal");
                inputVec.y = Input.GetAxisRaw("Vertical");
                inputVec = inputVec.normalized;
            }
        }

        private void FixedUpdate()
        {
            // FixedUpdate에서도 GameOver 여부 재확인하여 이동을 확실히 멈춤
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                if (rigid != null)
                    rigid.velocity = Vector2.zero;
                return;
            }

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

            StartHitFlash();

            if (currentHp <= 0)
            {
                currentHp = 0;
                Die();
            }
        }

        private void StartHitFlash()
        {
            if (material == null) return;

            if (hitFlashRoutine != null)
            {
                StopCoroutine(hitFlashRoutine);
                hitFlashRoutine = null;
            }

            hitFlashRoutine = StartCoroutine(HitFlashCoroutine());
        }

        private IEnumerator HitFlashCoroutine()
        {
            material.SetFloat("_HitFlash", 1f);

            float t = 0f;
            while (t < hitFlashDuration)
            {
                t += Time.deltaTime;
                yield return null;
            }

            material.SetFloat("_HitFlash", 0f);


            hitFlashRoutine = null;
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
            if (material != null)
                material.SetFloat("_Invincible", 1f);

            yield return new WaitForSecondsRealtime(duration);

            if (material != null)
                material.SetFloat("_Invincible", 0f);

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
            rigid.velocity = Vector2.zero;
        }

        public void StartContactDamage(GameObject enemySource, int damage, float tick)
        {
            if (enemySource == null) return;
            if (_contactCoroutines.ContainsKey(enemySource)) return;

            Coroutine c = StartCoroutine(ContactDamageCoroutine(enemySource, damage, tick));
            _contactCoroutines[enemySource] = c;
        }

        public void StopContactDamage(GameObject enemySource)
        {
            if (enemySource == null) return;
            if (_contactCoroutines.TryGetValue(enemySource, out var c))
            {
                if (c != null)
                    StopCoroutine(c);
                _contactCoroutines.Remove(enemySource);
            }
        }

        private IEnumerator ContactDamageCoroutine(GameObject source, int damage, float tick)
        {
            if (source == null || !source.activeInHierarchy) yield break;

            TakeDamage(damage);

            while (true)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                    break;

                if (source == null || !source.activeInHierarchy) break;

                yield return new WaitForSeconds(tick);

                TakeDamage(damage);
            }

            _contactCoroutines.Remove(source);
        }

        private void OnDestroy()
        {
            if (hitFlashRoutine != null)
                StopCoroutine(hitFlashRoutine);

            if (invincibleRoutine != null)
                StopCoroutine(invincibleRoutine);

            if (Instance == this)
                Instance = null;
        }
    }
}