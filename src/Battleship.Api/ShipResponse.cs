using Battleship.Domain;

namespace Battleship.Api;

public sealed record ShipResponse(
    ShipType ShipType,
    IReadOnlyList<string> Coordinates,
    bool IsSunk);