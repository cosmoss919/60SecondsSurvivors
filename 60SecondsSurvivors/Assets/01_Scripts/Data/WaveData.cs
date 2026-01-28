using System;
using UnityEngine;

namespace _60SecondsSurvivors.Data
{
    [CreateAssetMenu(menuName = "60SecondsSurvivors/Data/Wave Data", fileName = "WaveData_")]
    public class WaveData : ScriptableObject
    {
        [Serializable]
        public struct WeightedEnemy
        {
            public EnemyData enemy;

            [Min(0f)]
            public float weight;
        }

        [Serializable]
        public struct Phase
        {
            [Tooltip("이 구간 시작 시간(초) - 예: 0")]
            public float startTime;

            [Tooltip("이 구간 종료 시간(초) - 예: 20")]
            public float endTime;

            [Tooltip("스폰 간격(초)")]
            public float spawnInterval;

            [Header("Enemy")]
            [Tooltip("단일 적, enemies가 비어있으면 이 값을 사용")]
            public EnemyData enemy;

            [Tooltip("여러 적을 랜덤 스폰하고 싶으면 여기에 넣으세요(가중치).")]
            public WeightedEnemy[] enemies;
        }

        public Phase[] phases;

        public bool TryGetPhase(float elapsedTime, out Phase phase)
        {
            if (phases == null)
            {
                phase = default;
                return false;
            }

            for (int i = 0; i < phases.Length; i++)
            {
                var p = phases[i];
                if (elapsedTime >= p.startTime && elapsedTime < p.endTime)
                {
                    phase = p;
                    return true;
                }
            }

            phase = default;
            return false;
        }

        public bool TryPickEnemy(float elapsedTime, out EnemyData enemyData, out float spawnInterval)
        {
            enemyData = null;
            spawnInterval = 0f;

            if (!TryGetPhase(elapsedTime, out var phase))
                return false;

            spawnInterval = phase.spawnInterval;

            // 1) enemies 목록이 있으면 가중치 랜덤
            if (phase.enemies != null && phase.enemies.Length > 0)
            {
                float total = 0f;
                for (int i = 0; i < phase.enemies.Length; i++)
                {
                    var w = phase.enemies[i].weight;
                    if (w > 0f && phase.enemies[i].enemy != null)
                        total += w;
                }

                if (total > 0f)
                {
                    float roll = UnityEngine.Random.value * total;
                    float acc = 0f;

                    for (int i = 0; i < phase.enemies.Length; i++)
                    {
                        var e = phase.enemies[i].enemy;
                        var w = phase.enemies[i].weight;
                        if (e == null || w <= 0f) continue;

                        acc += w;
                        if (roll <= acc)
                        {
                            enemyData = e;
                            return true;
                        }
                    }
                }
            }

            // 2) fallback: 단일 enemy
            enemyData = phase.enemy;
            return enemyData != null;
        }
    }
}

