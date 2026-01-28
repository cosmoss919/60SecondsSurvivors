using UnityEngine;
using TMPro;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _resultText;

        private void Start()
        {
            if (_resultText != null)
            {
                _resultText.text = GameResult.IsWin ? "CLEAR" : "GAME OVER";
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

