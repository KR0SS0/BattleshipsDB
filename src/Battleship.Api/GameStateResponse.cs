using Battleship.Domain;

namespace Battleship.Api;

public sealed record GameStateResponse(
    Guid GameId,
    Side Turn,
    Side? Winner,
    Difficulty Difficulty,
    PlayerBoardResponse PlayerBoard,
    OpponentBoardResponse OpponentBoard) {

    public static GameStateResponse From(Game game) =>
        new(
            game.Id,
            game.CurrentTurn,
            game.Winner,
            game.Difficulty,
            new PlayerBoardResponse(
                ShotsReceived: ToShotResponses(game.PlayerBoard),
                Ships: game.PlayerBoard.Ships
                    .OrderBy(ship => ship.Type)
                    .Select(ship => ToShipResponse(game.PlayerBoard, ship))
                    .ToList()),
            new OpponentBoardResponse(
                ShotsReceived: ToShotResponses(game.OpponentBoard),
                SunkShips: game.OpponentBoard.Ships
                    .Where(game.OpponentBoard.IsShipSunk)
                    .Select(ship => ship.Type)
                    .ToList()));

    private static List<ShotResponse> ToShotResponses(Board board) =>
        board.ShotCells
            .OrderBy(cell => cell.Row)
            .ThenBy(cell => cell.Column)
            .Select(cell => new ShotResponse(cell.ToString(), board.ShipAtCoordinate(cell) is not null))
            .ToList();

    private static ShipResponse ToShipResponse(Board board, Ship ship) =>
        new(
            ship.Type,
            ship.GetCells().Select(cell => cell.ToString()).ToList(),
            board.IsShipSunk(ship));
}

