namespace _60SecondsSurvivors.Core
{
    public static class GameResult
    {
        public static bool IsWin { get; private set; }

        public static void Set(bool isWin)
        {
            IsWin = isWin;
        }
    }
}

