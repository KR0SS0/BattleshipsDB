namespace Battleship.Domain.Tests;

public class ShotStrategySimulationTests
{
    private const int GamesPerStrategy = 250;

    [Fact]
    public void HuntTarget_NeedsFewerShotsThanRandom()
    {
        // Arrange
        var randomStrategy = new RandomShotStrategy();
        var huntTargetStrategy = new HuntTargetShotStrategy();

        // Act
        double averageRandom = AverageShotsToSinkFleet(randomStrategy);
        double averageHuntTarget = AverageShotsToSinkFleet(huntTargetStrategy);

        // Assert
        TestContext.Current.TestOutputHelper?.WriteLine($"Average random strategy amount = {averageRandom:F1}\n" +
            $"Average hunt target strategy amount = {averageHuntTarget:F1}");
        Assert.True(averageHuntTarget < averageRandom);
    }

    private static double AverageShotsToSinkFleet(IShotStrategy strategy) =>
        Enumerable.Range(0, GamesPerStrategy).Average(seed => CountShotsToSinkFleet(strategy, seed));

    private static int CountShotsToSinkFleet(IShotStrategy strategy, int seed)
    {
        var board = new Board();
        Assert.True(board.TryPlaceShipsRandomly(new Random(seed)));
        var random = new Random(seed);

        while (!board.AreAllShipsSunk)
        {
            var target = Assert.NotNull(strategy.ChooseTarget(board.ShotHistory, random));
            Assert.True(board.ReceiveShot(target).IsSuccess);
        }

        return board.ShotHistory.Count;
    }
}
