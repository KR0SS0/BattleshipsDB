namespace Battleship.Domain;

public static class ShipTypeExtensions
{

    public static int Length(this ShipType shipType)
    {
        return shipType switch
        {
            ShipType.Carrier => 5,
            ShipType.Battleship => 4,
            ShipType.Cruiser => 3,
            ShipType.Submarine => 3,
            ShipType.Destroyer => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(shipType), shipType, null)
        };
    }
}