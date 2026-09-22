namespace Battleship.Domain;

public sealed record Shot(ShotOutcome Outcome, ShipType? SunkShipType = null)
{
    public static Shot Miss() => new(ShotOutcome.Miss);
 
    public static Shot Hit() => new(ShotOutcome.Hit);

    public static Shot Sunk(ShipType shipType) => new(ShotOutcome.Sunk, shipType);

}