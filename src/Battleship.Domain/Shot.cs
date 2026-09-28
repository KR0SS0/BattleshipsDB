namespace Battleship.Domain;

public sealed record Shot(Coordinate Coordinate, ShotOutcome Outcome, ShipType? SunkShipType = null)
{
    public static Shot Miss(Coordinate coordinate) => new(coordinate, ShotOutcome.Miss);

    public static Shot Hit(Coordinate coordinate) => new(coordinate, ShotOutcome.Hit);

    public static Shot Sunk(Coordinate coordinate, ShipType shipType) => new(coordinate, ShotOutcome.Sunk, shipType);
}
