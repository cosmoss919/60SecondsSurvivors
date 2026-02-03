using UnityEngine;
using _60SecondsSurvivors.Projectile;
using _60SecondsSurvivors.Enemy;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private ProjectileBase projectilePrefab;
        [SerializeField] private float fireInterval = 0.5f;
        private float timer;
        private Vector2 lastDirection = Vector2.right;

        // 버프 상태
        private float damageMultiplier = 1f;
        private float fireRateMultiplier = 1f;
        private int extraProjectiles = 0;
        private int pierceCount = 0;

        [Header("Aim Optimization")]
        [SerializeField, Range(1, 30)]
        private int aimUpdateFrequency = 4; 
        [SerializeField] private float aimMaxDistance = 30f;
        private int _aimFrameCounter;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            _aimFrameCounter++;
            if (_aimFrameCounter >= aimUpdateFrequency)
            {
                _aimFrameCounter = 0;
                UpdateAimToNearestEnemy();
            }

            timer += Time.deltaTime;
            if (timer >= fireInterval * fireRateMultiplier)
            {
                timer = 0f;
                Shoot();
            }
        }

        private void UpdateAimToNearestEnemy()
        {
            var manager = EnemyManager.Instance ?? EnemyManager.EnsureExists();
            if (manager == null) return;

            Vector2 selfPos = transform.position;
            var nearest = manager.GetNearest(selfPos, aimMaxDistance);
            if (nearest != null)
            {
                Vector2 dir = (nearest.transform.position - transform.position);
                if (dir == Vector2.zero) dir = Vector2.right;
                lastDirection = dir.normalized;
            }
        }

        private void Shoot()
        {
            if (projectilePrefab == null)
                return;

            int total = 1 + Mathf.Max(0, extraProjectiles);

            float baseAngle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            // 가운데 정렬 스프레드
            float spread = 10f;
            float startAngle = baseAngle - spread * (total - 1) / 2f;

            SoundManager.Instance?.PlayPlayerShoot();

            for (int i = 0; i < total; i++)
            {
                GameObject go;
                if (PoolManager.Instance != null)
                {
                    go = PoolManager.Instance.GetFromPool(projectilePrefab);
                }
                else
                {
                    go = Instantiate(projectilePrefab.gameObject);
                }

                if (go == null) continue;

                var proj = go.GetComponent<ProjectileBase>();
                if (proj == null) continue;

                // 위치/회전/방향 설정
                proj.transform.position = transform.position;
                float angle = startAngle + spread * i;
                proj.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

                // 데미지/관통 등 버프 적용
                proj.MultiplyDamage(damageMultiplier);
                proj.SetPierce(pierceCount);

                // 방향은 회전값 기준으로 계산
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                proj.SetDirection(dir.normalized);
            }
        }

        public void MultiplyDamage(float factor)
        {
            damageMultiplier *= factor;
        }

        public void MultiplyFireRate(float factor)
        {
            fireRateMultiplier *= factor;
        }

        public void AddProjectileCount(int add)
        {
            extraProjectiles += add;
            if (extraProjectiles < 0) extraProjectiles = 0;
        }

        public void AddPierce(int add)
        {
            pierceCount += add;
            if (pierceCount < 0) pierceCount = 0;
        }
    }
}