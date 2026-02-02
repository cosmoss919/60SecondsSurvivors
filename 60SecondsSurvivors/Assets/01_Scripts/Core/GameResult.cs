namespace _60SecondsSurvivors.Core
{
    public static class GameResult
    {
        public static int Score { get; private set; }

        public static void Set(int score)
        {
            Score = score;
        }
    }
}

