namespace Battleship.Domain;

public static class PlacementErrors
{
    public static readonly Error OutOfBounds = new(ErrorType.Invalid, "placement.out_of_bounds", "Ship extends outside the board.");
    public static readonly Error DuplicateShipType = new(ErrorType.Conflict, "placement.duplicate_shiptype", "A ship of this type is already placed.");
    public static readonly Error Overlap = new(ErrorType.Conflict, "placement.overlap", "Ship overlaps another ship.");
}