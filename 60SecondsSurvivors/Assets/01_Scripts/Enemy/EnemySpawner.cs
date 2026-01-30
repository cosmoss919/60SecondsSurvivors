using System.Collections.Generic;
using UnityEngine;
using _60SecondsSurvivors.Core;
using _60SecondsSurvivors.Data;
using _60SecondsSurvivors.Player;

namespace _60SecondsSurvivors.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private WaveData waveData;

        [SerializeField] private float spawnDistanceX = 7f;
        [SerializeField] private float spawnDistanceY = 16f;

        private float timer;

        private void Start()
        {
            if (waveData != null && PoolManager.Instance != null)
            {
                var prefabs = new HashSet<EnemyHealth>();
                var phases = waveData.phases;
                if (phases != null)
                {
                    for (int i = 0; i < phases.Length; i++)
                    {
                        var p = phases[i];

                        if (p.enemy != null && p.enemy.prefab != null)
                            prefabs.Add(p.enemy.prefab);

                        if (p.enemies != null)
                        {
                            for (int j = 0; j < p.enemies.Length; j++)
                            {
                                var e = p.enemies[j].enemy;
                                if (e != null && e.prefab != null)
                                    prefabs.Add(e.prefab);
                            }
                        }
                    }
                }

                foreach (var prefab in prefabs)
                {
                    PoolManager.Instance.Preload(prefab);
                }
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            if (waveData == null || timeManager == null)
                return;

            if (!waveData.TryPickEnemy(timeManager.ElapsedTime, out var enemyData, out var waveInterval))
                return;

            float interval = Mathf.Max(0.01f, waveInterval);
            EnemyHealth prefab = enemyData?.prefab;
            if (prefab == null) return;

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                TrySpawn(prefab);
            }
        }

        private void TrySpawn(EnemyHealth prefab)
        {
            if (prefab == null) return;

            Vector3 spawnPos = Vector3.zero;

            float signX = Random.value < 0.5f ? -1f : 1f;
            float signY = Random.value < 0.5f ? -1f : 1f;

            float offsetX = signX * (spawnDistanceX + Random.Range(-3f, 3f));
            float offsetY = signY * (spawnDistanceY + Random.Range(-3f, 3f));

            spawnPos = PlayerController.Instance.Position + new Vector2(offsetX, offsetY);

            var go = PoolManager.Instance != null ? PoolManager.Instance.GetFromPool(prefab) : Instantiate(prefab.gameObject);
            if (go == null) return;

            go.transform.Translate(spawnPos);
            go.transform.rotation = Quaternion.identity;
        }
    }
}

