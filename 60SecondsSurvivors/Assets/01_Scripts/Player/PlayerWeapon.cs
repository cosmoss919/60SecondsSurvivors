using UnityEngine;
using _60SecondsSurvivors.Projectile;
using _60SecondsSurvivors.Enemy;

namespace _60SecondsSurvivors.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private ProjectileBase projectilePrefab;
        [SerializeField] private float fireInterval = 0.5f;
        private float timer;
        private Vector2 lastDirection = Vector2.right;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= fireInterval)
            {
                timer = 0f;
                Shoot();
            }
        }

        private void FixedUpdate()
        {
            UpdateAimToNearestEnemy();
        }

        private void UpdateAimToNearestEnemy()
        {
            var enemies = FindObjectsOfType<EnemyHealth>();
            if (enemies == null || enemies.Length == 0)
                return;

            Vector2 selfPos = transform.position;
            EnemyHealth nearest = null;
            float bestDistSq = float.MaxValue;

            for (int i = 0; i < enemies.Length; i++)
            {
                var e = enemies[i];
                if (e == null || !e.gameObject.activeInHierarchy)
                    continue;

                Vector2 ePos = e.transform.position;
                float distSq = (ePos - selfPos).sqrMagnitude;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    nearest = e;
                }
            }

            if (nearest != null)
            {
                Vector2 dir = (nearest.transform.position - transform.position);
                if (dir == Vector2.zero)
                    dir = Vector2.right;
                lastDirection = dir.normalized;
            }
        }

        private void Shoot()
        {
            if (projectilePrefab == null)
                return;

            var projectile = Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
            Vector2 dir = lastDirection;
            if (dir == Vector2.zero) dir = Vector2.right;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = projectile.transform.rotation * Quaternion.Euler(0f, 0f, angle);
            projectile.SetDirection(dir.normalized);
        }
    }
}