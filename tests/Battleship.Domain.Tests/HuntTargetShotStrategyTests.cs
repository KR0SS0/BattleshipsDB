namespace Battleship.Domain.Tests;

public class HuntTargetShotStrategyTests
{
    [Fact]
    public void CellCandidates_HitInTheMiddle_ReturnsItsFourNeighbours()
    {
        // Arrange
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(4, 4))];

        // Act
        var candidates = HuntTargetShotStrategy.CellCandidates(shotHistory);

        // Assert
        Assert.Equal(4, candidates.Count);
        Assert.Contains(new Coordinate(3, 4), candidates);
        Assert.Contains(new Coordinate(5, 4), candidates);
        Assert.Contains(new Coordinate(4, 3), candidates);
        Assert.Contains(new Coordinate(4, 5), candidates);
    }

    [Fact]
    public void CellCandidates_HitInTheCorner_SkipsCellsOffTheBoard()
    {
        // Arrange
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(0, 0))];

        // Act
        var candidates = HuntTargetShotStrategy.CellCandidates(shotHistory);

        // Assert
        Assert.Equal(2, candidates.Count);
        Assert.Contains(new Coordinate(1, 0), candidates);
        Assert.Contains(new Coordinate(0, 1), candidates);
    }

    [Fact]
    public void CellCandidates_NeighbourAlreadyShot_IsSkipped()
    {
        // Arrange
        List<Shot> shotHistory = [Shot.Miss(new Coordinate(5, 4)), Shot.Hit(new Coordinate(4, 4))];

        // Act
        var candidates = HuntTargetShotStrategy.CellCandidates(shotHistory);

        // Assert
        Assert.Equal(3, candidates.Count);
        Assert.DoesNotContain(new Coordinate(5, 4), candidates);
    }

    [Fact]
    public void CellCandidates_OnlyHitsOnASunkShip_ReturnsNoCandidates()
    {
        // Arrange
        var destroyer = new Ship(ShipType.Destroyer, new Coordinate(3, 3), Orientation.Horizontal);
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(3, 3)), Shot.Sunk(new Coordinate(4, 3), destroyer)];

        // Act
        var candidates = HuntTargetShotStrategy.CellCandidates(shotHistory);

        // Assert
        Assert.Empty(candidates);
    }

    [Fact]
    public void ChooseTarget_AfterAHit_ShootsANeighbour()
    {
        // Arrange
        var shooter = new HuntTargetShotStrategy();
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(4, 4))];

        // Act
        var target = shooter.ChooseTarget(shotHistory, new Random(1));

        // Assert
        Assert.Contains(target!.Value, HuntTargetShotStrategy.CellCandidates(shotHistory));
    }
}
