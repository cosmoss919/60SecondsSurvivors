using UnityEngine;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.UI
{
    public class TitleUI : MonoBehaviour
    {
        public void OnClickStart()
        {
            SceneLoader.LoadGameScene();
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }
    }
}

