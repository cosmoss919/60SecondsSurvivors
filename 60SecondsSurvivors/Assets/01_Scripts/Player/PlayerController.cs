using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _60SecondsSurvivors.Core;
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
        [SerializeField] private VirtualJoystick joystick;

        private float hitFlashDuration = 0.2f;
        private float currentHp;
        private Animator animator;
        private Rigidbody2D rigid;
        private SpriteRenderer spriteRenderer;
        private Material material;
        private Vector2 inputVec;

        private bool isInvincible;
        private Coroutine invincibleRoutine;

        private Coroutine hitFlashRoutine;

        private readonly Dictionary<GameObject, Coroutine> contactCoroutines = new();

        public event Action<int, int> OnHpChanged;

        private void Awake()
        {
            currentHp = (int)maxHp;
            animator = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                material = spriteRenderer.material;
            }

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            OnHpChanged?.Invoke((int)currentHp, (int)maxHp);
        }

        private void Update()
        {
            if (joystick != null && joystick.Direction != Vector2.zero)
            {
                inputVec = joystick.Direction;
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

            SoundManager.Instance?.PlayPlayerHit();

            currentHp -= amount;
            if (currentHp < 0) currentHp = 0;

            OnHpChanged?.Invoke((int)currentHp, (int)maxHp);

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
            if (currentHp > maxHp) currentHp = (int)maxHp;

            OnHpChanged?.Invoke((int)currentHp, (int)maxHp);
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

            yield return new WaitForSeconds(duration);

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

            foreach (var kv in contactCoroutines)
            {
                if (kv.Value != null)
                    StopCoroutine(kv.Value);
            }
            contactCoroutines.Clear();

            animator.SetTrigger("Dead");
            rigid.velocity = Vector2.zero;

            SoundManager.Instance?.PlayPlayerDead();
        }

        public void StartContactDamage(GameObject enemySource, int damage, float tick)
        {
            if (enemySource == null) return;
            if (contactCoroutines.ContainsKey(enemySource)) return;

            Coroutine c = StartCoroutine(ContactDamageCoroutine(enemySource, damage, tick));
            contactCoroutines[enemySource] = c;
        }

        public void StopContactDamage(GameObject enemySource)
        {
            if (enemySource == null) return;
            if (contactCoroutines.TryGetValue(enemySource, out var c))
            {
                if (c != null)
                    StopCoroutine(c);
                contactCoroutines.Remove(enemySource);
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

            contactCoroutines.Remove(source);
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