namespace Battleship.Domain;

public static class DifficultyExtension {
    public static IShotStrategy ShotStrategy(this Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => new RandomShotStrategy(),
            Difficulty.Normal => new HuntTargetShotStrategy(),
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };
    }
}
