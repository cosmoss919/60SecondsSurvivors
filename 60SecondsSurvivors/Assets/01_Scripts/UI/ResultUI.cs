using UnityEngine;
using TMPro;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text scoreText;
        private const string PrefKeyHighScore = "60SS_HighScore";

        private void Start()
        {
            int highScore = PlayerPrefs.GetInt(PrefKeyHighScore, 0);
            if (highScoreText != null)
            {
                highScoreText.text = $"High Score : {highScore}";
            }
            if (scoreText != null)
            {
                scoreText.text = $"Score : {GameResult.Score}";
            }
        }

        public void OnClickRetry()
        {
            SceneLoader.LoadGameScene();
        }

        public void OnClickTitle()
        {
            SceneLoader.LoadTitleScene();
        }
    }
}

