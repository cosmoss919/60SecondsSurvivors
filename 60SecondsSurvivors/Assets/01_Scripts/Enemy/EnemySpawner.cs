using UnityEngine;
using _60SecondsSurvivors.Core;
using _60SecondsSurvivors.Data;

namespace _60SecondsSurvivors.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private WaveData _waveData;

        [Header("Fallback (WaveData 없을 때)")]
        [SerializeField] private EnemyHealth _enemyPrefab;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private Transform[] _spawnPoints;

        private float _timer;

        private void Update()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
                return;

            float interval = _spawnInterval;
            EnemyHealth prefab = _enemyPrefab;

            if (_timeManager != null && _waveData != null && _waveData.TryPickEnemy(_timeManager.ElapsedTime, out var enemyData, out var waveInterval))
            {
                interval = Mathf.Max(0.01f, waveInterval);
                if (enemyData != null && enemyData.prefab != null)
                    prefab = enemyData.prefab;
            }

            if (prefab == null) return;

            _timer += Time.deltaTime;
            if (_timer >= interval)
            {
                _timer = 0f;
                Spawn(prefab);
            }
        }

        private void Spawn(EnemyHealth prefab)
        {
            var index = Random.Range(0, _spawnPoints.Length);
            var point = _spawnPoints[index];

            Instantiate(prefab, point.position, Quaternion.identity);
        }
    }
}

