using UnityEngine;
using _60SecondsSurvivors.Projectile;

namespace _60SecondsSurvivors.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private ProjectileBase _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _fireInterval = 0.5f;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _fireInterval)
            {
                _timer = 0f;
                Shoot();
            }
        }

        private void Shoot()
        {
            if (_projectilePrefab == null || _firePoint == null)
                return;

            var projectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
            projectile.SetDirection(_firePoint.forward); // firePoint의 오른쪽 방향으로 발사
        }
    }
}

