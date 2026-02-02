using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 점수 집계 및 최고점 관리
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private int _currentScore;
        private int _highScore;

        private const string PrefKeyHighScore = "60SS_HighScore";

        public int CurrentScore => _currentScore;
        public int HighScore => _highScore;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _highScore = PlayerPrefs.GetInt(PrefKeyHighScore, 0);
            _currentScore = 0;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }   

        public void ResetRun()
        {
            _currentScore = 0;
        }

        public void AddScore(int amount)
        {
            if (amount <= 0) return;
            _currentScore += amount;
        }

        public void SaveHighScore()
        {
            if (_currentScore > _highScore)
            {
                _highScore = _currentScore;
                PlayerPrefs.SetInt(PrefKeyHighScore, _highScore);
                PlayerPrefs.Save();
            }
        }
    }
}