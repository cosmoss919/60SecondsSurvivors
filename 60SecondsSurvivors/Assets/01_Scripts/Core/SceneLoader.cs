using UnityEngine.SceneManagement;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 씬 전환 매니저
    /// </summary>
    public static class SceneLoader
    {
        private const string TitleSceneName = "TitleScene";
        private const string GameSceneName = "GameScene";
        private const string ResultSceneName = "ResultScene";

        public static void LoadTitleScene()
        {
            SceneManager.LoadScene(TitleSceneName);
        }

        public static void LoadGameScene()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PrepareNewRun();
            }
            SceneManager.LoadScene(GameSceneName);
        }

        public static void LoadResultScene(bool isWin)
        {
            GameResult.Set(isWin);
            SceneManager.LoadScene(ResultSceneName);
        }
    }
}

