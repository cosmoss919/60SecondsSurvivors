using System;
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

        // 이벤트: 초 단위가 바뀔 때 호출 (남은 초)
        public event Action<int> OnSecondTick;

        private int _lastReportedSecond = -1;

        private void Start()
        {
            StartTimer();
        }

        public void StartTimer()
        {
            _remainingTime = _gameDuration;
            _isRunning = true;
            // 초기 리포트
            _lastReportedSecond = Mathf.CeilToInt(_remainingTime);
            OnSecondTick?.Invoke(_lastReportedSecond);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            if (!_isRunning) return;

            _remainingTime -= Time.deltaTime;
            if (_remainingTime < 0f) _remainingTime = 0f;

            int currentSecond = Mathf.CeilToInt(_remainingTime);
            if (currentSecond != _lastReportedSecond)
            {
                _lastReportedSecond = currentSecond;
                OnSecondTick?.Invoke(currentSecond);
            }

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

