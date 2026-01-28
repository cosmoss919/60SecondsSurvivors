using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    /// <summary>
    /// 공통 로그/에러 헬퍼
    /// </summary>
    public static class GameLog
    {
        public static void Error(MonoBehaviour context, string message)
        {
            Debug.LogError($"{message}", context);
        }

        public static void Warning(MonoBehaviour context, string message)
        {
            Debug.LogWarning($"{message}", context);
        }

        public static void Info(MonoBehaviour context, string message)
        {
            Debug.Log($"{message}", context);
        }
    }
}

