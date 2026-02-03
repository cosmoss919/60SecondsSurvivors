using System;
using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 점수 집계 및 최고점 관리
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private int currentScore;
        private int highScore;

        private const string PrefKeyHighScore = "60SS_HighScore";

        public int CurrentScore => currentScore;
        public int HighScore => highScore;

        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            highScore = PlayerPrefs.GetInt(PrefKeyHighScore, 0);
            currentScore = 0;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ResetRun()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void AddScore(int amount)
        {
            if (amount <= 0) return;
            currentScore += amount;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void SaveHighScore()
        {
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt(PrefKeyHighScore, highScore);
                PlayerPrefs.Save();
            }
        }
    }
}