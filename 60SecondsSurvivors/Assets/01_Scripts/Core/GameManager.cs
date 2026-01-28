using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 전체 게임 흐름을 관리하는 매니저
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private bool _isGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PrepareNewRun()
        {
            _isGameOver = false;
        }

        public void OnPlayerDied()
        {
            if (_isGameOver) return;

            _isGameOver = true;
            SceneLoader.LoadResultScene(false);
        }

        public void OnTimeOver()
        {
            if (_isGameOver) return;

            _isGameOver = true;
            SceneLoader.LoadResultScene(true);
        }
    }
}

