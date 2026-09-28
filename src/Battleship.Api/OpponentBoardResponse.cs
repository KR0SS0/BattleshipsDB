using Battleship.Domain;

namespace Battleship.Api;

public sealed record OpponentBoardResponse(
    IReadOnlyList<ShotResponse> ShotsReceived,
    IReadOnlyList<ShipType> SunkShips);