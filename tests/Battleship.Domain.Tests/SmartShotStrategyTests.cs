namespace Battleship.Domain.Tests;

public class SmartShotStrategyTests
{
    [Fact]
    public void CellCandidates_TwoHitsInARow_ReturnsOnlyTheCellsPastBothEnds()
    {
        // Arrange: D5 and E5
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(3, 4)), Shot.Hit(new Coordinate(4, 4))];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert: C5 and F5, not the cells above or below
        Assert.Equal(2, candidates.Count);
        Assert.Contains(new Coordinate(2, 4), candidates);
        Assert.Contains(new Coordinate(5, 4), candidates);
    }

    [Fact]
    public void CellCandidates_TwoHitsInAColumn_ReturnsOnlyTheCellsAboveAndBelow()
    {
        // Arrange: E4 and E5
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(4, 3)), Shot.Hit(new Coordinate(4, 4))];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert: E3 and E6
        Assert.Equal(2, candidates.Count);
        Assert.Contains(new Coordinate(4, 2), candidates);
        Assert.Contains(new Coordinate(4, 5), candidates);
    }

    [Fact]
    public void CellCandidates_ThreeHitsInARow_ReturnsTheCellsPastTheWholeLine()
    {
        // Arrange: C5, D5 and E5, fired out of order
        List<Shot> shotHistory =
        [
            Shot.Hit(new Coordinate(3, 4)),
            Shot.Hit(new Coordinate(4, 4)),
            Shot.Hit(new Coordinate(2, 4))
        ];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert: B5 and F5
        Assert.Equal(2, candidates.Count);
        Assert.Contains(new Coordinate(1, 4), candidates);
        Assert.Contains(new Coordinate(5, 4), candidates);
    }

    [Fact]
    public void CellCandidates_LineAgainstTheEdge_ReturnsOnlyTheOpenEnd()
    {
        // Arrange: A5 and B5
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(0, 4)), Shot.Hit(new Coordinate(1, 4))];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert: only C5
        Assert.Equal(new Coordinate(2, 4), Assert.Single(candidates));
    }

    [Fact]
    public void CellCandidates_OneEndAlreadyMissed_ReturnsTheOtherEnd()
    {
        // Arrange: miss at C5, hits at D5 and E5
        List<Shot> shotHistory =
        [
            Shot.Miss(new Coordinate(2, 4)),
            Shot.Hit(new Coordinate(3, 4)),
            Shot.Hit(new Coordinate(4, 4))
        ];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert: only F5
        Assert.Equal(new Coordinate(5, 4), Assert.Single(candidates));
    }

    [Fact]
    public void CellCandidates_BothEndsMissed_FallsBackToTheNeighbours()
    {
        // Arrange: the hits D5 and E5 belong to two different ships lying side by side
        List<Shot> shotHistory =
        [
            Shot.Miss(new Coordinate(2, 4)),
            Shot.Miss(new Coordinate(5, 4)),
            Shot.Hit(new Coordinate(3, 4)),
            Shot.Hit(new Coordinate(4, 4))
        ];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert: the cells above and below both hits
        Assert.Equal(4, candidates.Count);
        Assert.Contains(new Coordinate(3, 3), candidates);
        Assert.Contains(new Coordinate(3, 5), candidates);
        Assert.Contains(new Coordinate(4, 3), candidates);
        Assert.Contains(new Coordinate(4, 5), candidates);
    }

    [Fact]
    public void CellCandidates_SingleHit_ReturnsItsFourNeighbours()
    {
        // Arrange
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(4, 4))];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert
        Assert.Equal(4, candidates.Count);
    }

    [Fact]
    public void CellCandidates_OnlyHitsOnASunkShip_ReturnsNoCandidates()
    {
        // Arrange
        var destroyer = new Ship(ShipType.Destroyer, new Coordinate(3, 3), Orientation.Horizontal);
        List<Shot> shotHistory = [Shot.Hit(new Coordinate(3, 3)), Shot.Sunk(new Coordinate(4, 3), destroyer)];

        // Act
        var candidates = SmartShotStrategy.CellCandidates(shotHistory);

        // Assert
        Assert.Empty(candidates);
    }
}
