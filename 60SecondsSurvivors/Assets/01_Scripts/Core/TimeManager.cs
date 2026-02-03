using System;
using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 생존 시간을 관리하는 타이머매니저
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private float gameDuration = 60f;

        private float remainingTime;
        private bool isRunning;

        public float GameDuration => gameDuration;
        public float RemainingTime => remainingTime;
        public float ElapsedTime => Mathf.Max(0f, gameDuration - remainingTime);

        // 이벤트: 초 단위가 바뀔 때 호출 (남은 초)
        public event Action<int> OnSecondTick;

        private int _lastReportedSecond = -1;

        private void Start()
        {
            StartTimer();
        }

        public void StartTimer()
        {
            remainingTime = gameDuration;
            isRunning = true;
            _lastReportedSecond = Mathf.CeilToInt(remainingTime);
            OnSecondTick?.Invoke(_lastReportedSecond);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            if (!isRunning) return;

            remainingTime -= Time.deltaTime;
            if (remainingTime < 0f) remainingTime = 0f;

            int currentSecond = Mathf.CeilToInt(remainingTime);
            if (currentSecond != _lastReportedSecond)
            {
                _lastReportedSecond = currentSecond;
                OnSecondTick?.Invoke(currentSecond);
            }

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isRunning = false;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnTimeOver();
                }
            }
        }
    }
}

