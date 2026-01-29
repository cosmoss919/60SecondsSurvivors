using UnityEngine;
using TMPro;
using _60SecondsSurvivors.Core;
using _60SecondsSurvivors.Player;

namespace _60SecondsSurvivors.UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private PlayerController playerController;

        [Header("Text")]
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _hpText;

        private void Update()
        {
            if (_timeManager != null && _timeText != null)
            {
                _timeText.text = $"TIME: {Mathf.CeilToInt(_timeManager.RemainingTime)}";
            }

            if (playerController != null && _hpText != null)
            {
                _hpText.text = $"HP: {playerController.CurrentHp}/{playerController.MaxHp}";
            }
        }
    }
}

