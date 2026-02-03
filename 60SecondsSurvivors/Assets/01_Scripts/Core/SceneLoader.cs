using UnityEngine.SceneManagement;

namespace _60SecondsSurvivors.Core
{
    public static class SceneLoader
    {
        private const string TitleSceneName = "TitleScene";
        private const string GameSceneName = "GameScene";
        private const string ResultSceneName = "ResultScene";

        public static void LoadTitleScene()
        {
            SoundManager.Instance?.PlayTitleBgm();
            SceneManager.LoadScene(TitleSceneName);
        }

        public static void LoadGameScene()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PrepareNewRun();
            }
            SoundManager.Instance?.PlayGameBgm();
            SceneManager.LoadScene(GameSceneName);
        }

        public static void LoadResultScene()
        {
            GameResult.Set(ScoreManager.Instance.CurrentScore);
            SoundManager.Instance?.StopMusic();
            SceneManager.LoadScene(ResultSceneName);
        }
    }
}

