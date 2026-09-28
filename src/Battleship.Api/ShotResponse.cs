namespace Battleship.Api;

public sealed record ShotResponse(
    string Coordinate,
    bool DidHitShip);