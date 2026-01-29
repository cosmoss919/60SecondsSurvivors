using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 생존 시간을 관리하는 타이머매니저
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private float _gameDuration = 60f;

        private float _remainingTime;
        private bool _isRunning;

        public float GameDuration => _gameDuration;
        public float RemainingTime => _remainingTime;
        public float ElapsedTime => Mathf.Max(0f, _gameDuration - _remainingTime);

        private void Start()
        {
            StartTimer();
        }

        public void StartTimer()
        {
            _remainingTime = _gameDuration;
            _isRunning = true;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            if (!_isRunning) return;

            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                _isRunning = false;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnTimeOver();
                }
            }
        }
    }
}

