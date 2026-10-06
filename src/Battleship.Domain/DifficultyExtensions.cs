namespace Battleship.Domain;

public static class DifficultyExtensions {
    public static IShotStrategy ShotStrategy(this Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => new RandomShotStrategy(),
            Difficulty.Normal => new HuntTargetShotStrategy(),
            Difficulty.Hard => new SmartShotStrategy(),
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };
    }
}
