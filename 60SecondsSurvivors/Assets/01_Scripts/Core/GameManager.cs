using System.Collections;
using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 전체 게임 흐름을 관리하는 매니저
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public bool IsGameOver => _isGameOver;
        private bool _isGameOver;

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
            if (Instance == this)
                Instance = null;
        }

        public void PrepareNewRun()
        {
            _isGameOver = false;
            ScoreManager.Instance?.ResetRun();
        }

        public void OnPlayerDied()
        {
            if (_isGameOver) return;

            _isGameOver = true;
            SoundManager.Instance?.PlayGameOver();
            StartCoroutine(DelayedLoadResult());
        }

        public void OnTimeOver()
        {
            if (_isGameOver) return;

            _isGameOver = true;
            SoundManager.Instance?.PlayGameClear();
            StartCoroutine(DelayedLoadResult());
        }

        private IEnumerator DelayedLoadResult()
        {
            ScoreManager.Instance?.SaveHighScore();
            yield return new WaitForSeconds(3f);
            SceneLoader.LoadResultScene();
        }
    }
}

