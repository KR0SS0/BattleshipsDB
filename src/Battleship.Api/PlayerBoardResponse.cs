namespace Battleship.Api;

public sealed record PlayerBoardResponse(
    IReadOnlyList<ShotResponse> ShotsReceived,
    IReadOnlyList<ShipResponse> Ships);