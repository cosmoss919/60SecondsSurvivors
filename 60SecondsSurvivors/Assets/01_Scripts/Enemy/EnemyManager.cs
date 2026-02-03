using System.Collections.Generic;
using UnityEngine;

namespace _60SecondsSurvivors.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        private readonly List<EnemyAI> _enemies = new List<EnemyAI>(128);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            _enemies.Clear();
        }

        public static EnemyManager EnsureExists()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("EnemyManager");
            Instance = go.AddComponent<EnemyManager>();
            return Instance;
        }

        public void Register(EnemyAI e)
        {
            if (e == null) return;
            if (_enemies.Contains(e)) return;
            _enemies.Add(e);
        }

        public void Unregister(EnemyAI e)
        {
            if (e == null) return;
            _enemies.Remove(e);
        }

        public IReadOnlyList<EnemyAI> GetAll() => _enemies;

        public EnemyAI GetNearest(Vector2 position, float maxDistance = -1f)
        {
            EnemyAI best = null;
            float bestSq = float.MaxValue;
            float maxSq = (maxDistance > 0f) ? maxDistance * maxDistance : float.MaxValue;

            // 단순 선형 탐색(효율성: 적 수가 매우 많아지면 공간 색인 고려)
            for (int i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (e == null || !e.gameObject.activeInHierarchy || !e.IsAlive) continue;

                float d = ((Vector2)e.transform.position - position).sqrMagnitude;
                if (d < bestSq && d <= maxSq)
                {
                    bestSq = d;
                    best = e;
                }
            }

            return best;
        }
    }
}