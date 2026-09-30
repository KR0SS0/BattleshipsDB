namespace Battleship.Domain;

public sealed record Shot(Coordinate Coordinate, ShotOutcome Outcome, Ship? SunkShip = null)
{
    public ShipType? SunkShipType => SunkShip?.Type;

    public static Shot Miss(Coordinate coordinate) => new(coordinate, ShotOutcome.Miss);

    public static Shot Hit(Coordinate coordinate) => new(coordinate, ShotOutcome.Hit);

    public static Shot Sunk(Coordinate coordinate, Ship ship) => new(coordinate, ShotOutcome.Sunk, ship);
}
