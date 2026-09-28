using Battleship.Domain;

namespace Battleship.Api.IntegrationTests;

public sealed class GameStateResponseTests
{
    [Fact]
    public void From_OpponentBoard_ShowsOnlySunkShips()
    {
        // Arrange
        var game = new Game();
        var opponentBoard = game.OpponentBoard;
        Assert.True(opponentBoard.PlaceShip(new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal)).IsSuccess);
        Assert.True(opponentBoard.PlaceShip(new Ship(ShipType.Cruiser, new Coordinate(0, 2), Orientation.Horizontal)).IsSuccess);

        opponentBoard.ReceiveShot(new Coordinate(0, 0));
        opponentBoard.ReceiveShot(new Coordinate(1, 0));
        opponentBoard.ReceiveShot(new Coordinate(0, 2));
        opponentBoard.ReceiveShot(new Coordinate(9, 9));

        // Act
        var response = GameStateResponse.From(game);

        // Assert
        Assert.Equal([ShipType.Destroyer], response.OpponentBoard.SunkShips);
        Assert.Equal(
            [
                new ShotResponse("A1", true),
                new ShotResponse("B1", true),
                new ShotResponse("A3", true),
                new ShotResponse("J10", false),
            ],
            response.OpponentBoard.ShotsReceived);
    }

    [Fact]
    public void From_PlayerBoard_ShowsOwnShipsWithCellsAndSunkState()
    {
        // Arrange
        var game = new Game();
        var playerBoard = game.PlayerBoard;
        Assert.True(playerBoard.PlaceShip(new Ship(ShipType.Destroyer, new Coordinate(5, 5), Orientation.Vertical)).IsSuccess);
        Assert.True(playerBoard.PlaceShip(new Ship(ShipType.Carrier, new Coordinate(0, 0), Orientation.Horizontal)).IsSuccess);

        playerBoard.ReceiveShot(new Coordinate(5, 5));
        playerBoard.ReceiveShot(new Coordinate(5, 6));

        // Act
        var response = GameStateResponse.From(game);

        // Assert
        Assert.Equal(game.Id, response.GameId);
        Assert.Equal(game.CurrentTurn, response.Turn);
        Assert.Null(response.Winner);

        Assert.Collection(response.PlayerBoard.Ships,
            carrier =>
            {
                Assert.Equal(ShipType.Carrier, carrier.ShipType);
                Assert.Equal(["A1", "B1", "C1", "D1", "E1"], carrier.Coordinates);
                Assert.False(carrier.IsSunk);
            },
            destroyer =>
            {
                Assert.Equal(ShipType.Destroyer, destroyer.ShipType);
                Assert.Equal(["F6", "F7"], destroyer.Coordinates);
                Assert.True(destroyer.IsSunk);
            });

        Assert.Equal(
            [new ShotResponse("F6", true), new ShotResponse("F7", true)],
            response.PlayerBoard.ShotsReceived);
    }
}
