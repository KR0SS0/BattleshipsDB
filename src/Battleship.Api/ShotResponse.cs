using Battleship.Domain;

namespace Battleship.Api;

public sealed record ShotResponse(
    string Coordinate,
    bool DidHitShip)
{
    public static ShotResponse From(Shot shot) =>
        new(shot.Coordinate.ToString(), shot.Outcome != ShotOutcome.Miss);
}
